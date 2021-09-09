using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Hosting;
using dotenv.net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Conventions;
using Pepper.Services.FGO;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Structures.External.FGO;
using Pepper.Structures.External.Osu;
using Qmmands;
using Serilog;
using Serilog.Templates;

await new HostBuilder()
    .UseSerilog((_, configuration) =>
        configuration
            .MinimumLevel.Debug()
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .WriteTo.Console(
                new ExpressionTemplate(
                    "[{@t:dd-MM-yy HH:mm:ss} {@l:u3}] [Thread {ThreadId,2}]{#if Contains(SourceContext, 'Service')} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]{#end} {@m:lj}\n{@x}"
                )
            )
    )
    .ConfigureServices(services =>
    {
        services.Configure<CommandServiceConfiguration>(config =>
        {
            config.DefaultRunMode = RunMode.Parallel;
            config.DefaultArgumentParser = new ArgumentParser();
            config.IgnoresExtraArguments = true;
            config.StringComparison = StringComparison.InvariantCultureIgnoreCase;
        });
    })
    .ConfigureAppConfiguration(app =>
    {
        DotEnv.Load();
        app.AddEnvironmentVariables("PEPPER_");
        ConventionRegistry.Register(
            "IgnoreExtraElements",
            new ConventionPack { new IgnoreExtraElementsConvention(true) },
            _ => true
        );
        
        var configUrl = Environment.GetEnvironmentVariable("PEPPER_CONFIG_URL");
        if (!string.IsNullOrWhiteSpace(configUrl))
        {
            var httpClient = new HttpClient();
            Log.Information($"Downloading JSON configuration from {configUrl}...");
            var config = httpClient.GetStreamAsync(configUrl).Result;
            app.AddJsonStream(config);
        }
        else
        {
            app.AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "config/config.json"));
        }
    })
    .ConfigureDiscordBot<Pepper.Pepper>((context, bot) =>
    {
        bot.Token = context.Configuration["DISCORD_TOKEN"];
        bot.Prefixes = context.Configuration.GetAllCommandPrefixes()
            .SelectMany(kv => kv.Value);
    })
    .UseDefaultServiceProvider(option => option.ValidateOnBuild = true)
    .RunConsoleAsync();

namespace Pepper
{
    public class Pepper : DiscordBot
    {
        public static readonly string VersionHash = typeof(Pepper)
            .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "GitHash")?.Value ?? "unknown";
        
        public Pepper(
            IOptions<DiscordBotConfiguration> options,
            ILogger<Pepper> logger,
            IServiceProvider services,
            DiscordClient client
        ) : base(options, logger, services, client) {}
        
        private static readonly Type[] DownleveledAttributes = { typeof(CategoryAttribute), typeof(PrefixCategoryAttribute) };

        // expose this for tests
        public static void DownlevelAttributes(ModuleBuilder moduleBuilder)
        {
            foreach (var command in CommandUtilities.EnumerateAllCommands(moduleBuilder))
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
            }
        }
        protected override void MutateModule(ModuleBuilder moduleBuilder)
        {
            DownlevelAttributes(moduleBuilder);
            foreach (var command in CommandUtilities.EnumerateAllCommands(moduleBuilder))
            {
                if (command.Parameters.Count == 0) continue;
                
                var lastNotFlag = command.Parameters
                    .LastOrDefault(param => !param.Attributes.OfType<FlagAttribute>().Any());
                if (lastNotFlag == null || lastNotFlag.IsRemainder) continue;
                Logger.LogDebug(
                    "Parameter \"{0}\" in command \"{1}\" is the last parameter - setting remainder attribute to True.",
                    lastNotFlag.Name, command.Aliases.First());
                lastNotFlag.IsRemainder = true;
            }
            
            base.MutateModule(moduleBuilder);
        }

        protected override LocalMessage FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            if (result is CommandNotFoundResult) return null!;
        
            var content = "I'm sorry, an error occurred.";
            var embed = new LocalEmbed
            {
                Fields = new List<LocalEmbedField>(),
                Footer = new LocalEmbedFooter { Text = $"Command : {context.Command.Name} | Prefix : {context.Prefix}" },
                Timestamp = DateTimeOffset.Now
            };

            switch (result)
            {
                case CommandExecutionFailedResult executionFailedResult:
                    var exception = executionFailedResult.Exception;
                    var stackTrace = exception.StackTrace!.Split('\n');
                    
                    content = "I'm sorry, an error occurred executing your command.";
                    embed.Description = $"```{exception.Message}\n{string.Join('\n', stackTrace.Take(4))}```";
                    break;
                case TypeParseFailedResult typeParseFailedResult:
                {
                    var parameter = typeParseFailedResult.Parameter;

                    var formatter = parameter.Attributes.OfType<FormatTypeParseFailureAttribute>().FirstOrDefault();
                    var res = formatter?.Format(typeParseFailedResult);
                    if (res != null) return res;

                    content = "I'm sorry, an error occurred parsing your argument.";
                    embed.Fields = new List<LocalEmbedField>
                    {
                        new() { Name = "Parameter", Value = $"Name : `{parameter.Name}`\nType : `{parameter.Type.Name}`" },
                        new() { Name = "Parsing value", Value = $"`{typeParseFailedResult.Value}`" }
                    };
                    if (!string.IsNullOrWhiteSpace(typeParseFailedResult.FailureReason))
                        embed.Fields.Add(new LocalEmbedField
                        {
                            Name = "Failure reason",
                            Value = typeParseFailedResult.FailureReason
                        });
                    break;
                }
                case ParameterChecksFailedResult parameterChecksFailedResult:
                {
                    var formatter = parameterChecksFailedResult.Parameter.Attributes.OfType<IParameterCheckWithFailureFormatter>()
                        .FirstOrDefault();
                    var formatted = formatter?.FormatFailure(parameterChecksFailedResult);
                    if (formatted != null) return formatted;

                    goto default;
                }
                case ChecksFailedResult checksFailedResult:
                {
                    switch (checksFailedResult.FailedChecks[0].Check)
                    {
                        case PrefixCheckAttribute:
                            return null!;
                        case RequireGuildWhitelistAttribute:
                            content = "This command is restricted (whitelisted on a per-guild basis), hence not callable from this guild.";
                            break;
                    }
                    
                    goto default;
                }
                default:
                    embed.Description = result.FailureReason;
                    break;
            };

            return new LocalMessage
            {
                Content = content,
                Embeds = new List<LocalEmbed> { embed },
            }.WithReply(context.Message.Id);
        }

        protected override ValueTask AddTypeParsersAsync(CancellationToken cancellationToken = default)
        {
            Commands.AddTypeParser(RulesetTypeParser.Instance);
            Commands.AddTypeParser(UsernameTypeParser.Instance);
            Commands.AddTypeParser(BeatmapOrSetResolvableTypeParser.Instance);
            Commands.AddTypeParser(BeatmapResolvableTypeParser.Instance);
            
            Commands.AddTypeParser(
                new ServantIdentityTypeParser(
                    Services.GetRequiredService<MasterDataService>(),
                    Services.GetRequiredService<IConfiguration>()
                )
            );
            
            Commands.AddTypeParser(
                new CraftEssenceIdentityTypeParser(
                    Services.GetRequiredService<MasterDataService>(),
                    Services.GetRequiredService<CraftEssenceNamingService>()
                )
            );
            
            return base.AddTypeParsersAsync(cancellationToken);
        }
    }
}