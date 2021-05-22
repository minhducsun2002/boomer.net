using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Interactivity.Pagination;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services.Osu;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Structures.Commands.Result;
using Pepper.Structures.External.Osu;
using Qmmands;
using Serilog;
using CommandContext = Pepper.Structures.CommandContext;


namespace Pepper.Services.Main
{
    public class CommandService : Service
    {
        private readonly Client client;
        public readonly Qmmands.CommandService QmmandService;
        private readonly IServiceProvider serviceProvider;
        private readonly InteractivityService interactivityService;
        
        /// <summary>
        /// A lookup table, mapping from a prefix to allowed categories.
        /// A prefix may map to multiple categories (there would be multiple entries).
        /// </summary>
        public readonly Dictionary<string, List<string>> CategoriesByAllowedPrefixes = new Dictionary<string, List<string>>();
        public readonly Dictionary<string, List<string>> AllowedPrefixesByCategories = new Dictionary<string, List<string>>();
        private readonly ILogger log = Log.Logger;
        private readonly Type[] DownleveledAttributes = { typeof(CategoryAttribute), typeof(PrefixCategoryAttribute) };

        public CommandService(IServiceProvider serv)
        {
            serviceProvider = serv;
            client = serv.GetRequiredService<Client>();
            var configuration = serv.GetRequiredService<Configuration>();
            interactivityService = serv.GetRequiredService<InteractivityService>();
            QmmandService = new Qmmands.CommandService(new CommandServiceConfiguration
            {
                DefaultRunMode = Qmmands.RunMode.Parallel,
                DefaultArgumentParser = new ArgumentParser(),
                IgnoresExtraArguments = true,
                StringComparison = StringComparison.InvariantCultureIgnoreCase
            });
            // TODO: Discover type parsers & add dynamically
            QmmandService.AddTypeParser(RulesetTypeParser.Instance);
            QmmandService.AddTypeParser(new UsernameTypeParser(serv.GetRequiredService<DiscordOsuUsernameLookupService>()));
            
            foreach (var mapping in configuration.Where(map => map.Key.StartsWith("command:prefix")))
            {
                var (configKey, prefixes) = mapping;
                var category = configKey.Split(':').Last();
                foreach (var prefix in prefixes)
                    if (!CategoriesByAllowedPrefixes.TryAdd(prefix, new List<string> { category }))
                        CategoriesByAllowedPrefixes[prefix].Add(category);

                if (!AllowedPrefixesByCategories.TryAdd(category, prefixes.ToList()))
                    AllowedPrefixesByCategories[category] =
                        AllowedPrefixesByCategories[category].Concat(prefixes).Distinct().ToList();
            }
        }

        public override Task Initialize()
        {
            QmmandService.AddModules(Assembly.GetEntryAssembly(), null, builder =>
            {
                foreach (var command in CommandUtilities.EnumerateAllCommands(builder))
                {
                    foreach (var attribute in DownleveledAttributes)
                        if (command.Attributes.All(attrib => attrib.GetType() != attribute))
                        {
                            var module = command.Module;
                            while (module != null)
                            {
                                var category = module.Attributes.FirstOrDefault(attrib => attrib.GetType() == attribute);
                                if (category != null)
                                {
                                    command.AddAttribute(category);
                                    break;
                                }
                                module = module.Parent;
                            }
                        }
                    
                    if (command.Parameters.Count > 0)
                    {
                        var lastNotFlag = command.Parameters.LastOrDefault(
                            param => !param.Attributes.OfType<FlagAttribute>().Any());
                        if (lastNotFlag == null || lastNotFlag.IsRemainder) continue;
                        log.Debug(
                            "Parameter \"{0}\" in command \"{1}\" is the last parameter - setting remainder attribute to True.",
                            lastNotFlag.Name, command.Aliases.First());
                        lastNotFlag.IsRemainder = true;
                    }
                }
            });
            var commands = QmmandService.GetAllCommands().ToArray();
            log.Information($"Loaded {commands.Length} commands.");
            foreach (var command in commands)
                log.Debug($"Loaded command {command.Name}.");
            
            client.MessageReceived += HandleMessage;
            QmmandService.CommandExecuted += HandleSuccessfulResult;
            QmmandService.CommandExecutionFailed += HandleFailedResult;
            
            if (CategoriesByAllowedPrefixes.Count == 0) return base.Initialize();
            log.Debug("Found customized prefixes for command categories.");
            log.Debug("Prefixes that can invoke respective command categories :");
            foreach (var (key, value) in CategoriesByAllowedPrefixes)
                log.Debug("  {0} => {1}", key, string.Join(',', value));
            
            return base.Initialize();
        }

        private async Task HandleSuccessfulResult(CommandExecutedEventArgs executedEventArgs)
        {
            var result = executedEventArgs.Result;
            var context = (executedEventArgs.Context as CommandContext)!;
            switch (result)
            {
                case TextResult textResult:
                {
                    await context.Channel.SendMessageAsync(textResult.MessageText.Length > 0 ? textResult.MessageText : "No comment.");
                    break;
                }
                case EmbedResult embedResult:
                {
                    var embeds = embedResult.Embeds;
                    if (embeds.Length > 1)
                        await interactivityService.SendPaginatorAsync(
                            new StaticPaginatorBuilder()
                                .WithEmotes(
                                    new Dictionary<IEmote, PaginatorAction>
                                    {
                                        {new Emoji("\u2B05"), PaginatorAction.Backward},
                                        {new Emoji("\u27A1"), PaginatorAction.Forward}
                                    })
                                .WithCancelledEmbed(null)
                                .WithTimoutedEmbed(null)
                                .WithDeletion(DeletionOptions.None)
                                .WithFooter(PaginatorFooter.None)
                                .WithPages(embeds.Select(PageBuilder.FromEmbed))
                                .Build(),
                            context.Channel,
                            TimeSpan.FromSeconds(20)
                        );

                    else
                        await context.Channel.SendMessageAsync("", false, embeds.Any() ? embeds[0] : embedResult.DefaultEmbed);

                    break;
                }
            }
        }

        private async Task HandleFailedResult(CommandExecutionFailedEventArgs executionFailedEventArgs)
        {
            var context = (CommandContext) executionFailedEventArgs.Context;
            var result = executionFailedEventArgs.Result;
            log.Error(result.Exception, "");
            await context.Channel.SendMessageAsync(
                "",
                true,
                new EmbedBuilder
                {
                    Title = $"Apologize, there was an error trying to execute command `{result.Command.Name}` : ",
                    Description =
                        $"```{result.FailureReason}```\n```{result.Exception.Message}\n{result.Exception.StackTrace}```",
                    Timestamp = DateTimeOffset.Now
                }.Build());
        }
        
        private async Task HandleMessage(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage msg)) return;
            if (msg.Author.Id == client.CurrentUser.Id) return;

            if (msg.Author.Id != 383990559070486529) return;

            var execTargetPosition = 0;
            var prefix = "";
            var validPrefix = CategoriesByAllowedPrefixes.FirstOrDefault(mapping =>
            {
                execTargetPosition = 0;
                return msg.HasStringPrefix(prefix = mapping.Key, ref execTargetPosition,
                    StringComparison.InvariantCultureIgnoreCase);
            });
            if (validPrefix.Equals(default)) return;

            var context = new CommandContext(serviceProvider)
            {   
                Client = client,
                Author = msg.Author,
                Channel = msg.Channel,
                Message = msg,
                CommandService = this,
                Prefix = prefix
            };

            var _ = await QmmandService.ExecuteAsync(msg.Content[execTargetPosition..], context);
            
            if (!_!.IsSuccessful)
                if (!(_ is CommandNotFoundResult))
                    Log.Error(_.ToString());
        }
    }
}