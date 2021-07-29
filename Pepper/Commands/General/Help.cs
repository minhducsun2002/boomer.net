using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using FuzzySharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Utilities;
using Qmmands;
using Command = Qmmands.Command;

namespace Pepper.Commmands.General
{
    public class Help : GeneralCommand
    {
        
        
        [Command("help")]
        [Description("Everything starts here.")]
        public DiscordCommandResult Exec(
            [Description("A command or a category to show respective help entry.")] string query = ""
            )
        {
            var self = Context.Bot.CurrentUser;
            var service = Context.Command.Service;
            var commands = service.GetAllCommands()!;
            var categories = commands
                .GroupBy(
                    command => command.Attributes.OfType<CategoryAttribute>().FirstOrDefault()?.Category ?? "",
                    command => command
                )
                .ToDictionary(group => group.Key, group => group.ToArray());

            var embedAuthor = new LocalEmbedAuthor
            {
                Name = $"{self.Name}#{self.Tag}",
                IconUrl = self.GetAvatarUrl(CdnAssetFormat.Png, 1024)
            };
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                var commandMatches = service.FindCommands(query);
                if (commandMatches.Count != 0) return Reply(HandleCommand(commandMatches[0].Command));
                
                var categoryMatches = Process.ExtractTop(
                    query.ToLowerInvariant(),
                    categories.Select(_ => _.Key),
                    s => s.ToLowerInvariant(),
                    limit: 1)
                    .ToArray();

                if (categoryMatches[0].Score >= 60)
                    return Reply(HandleCategory(categoryMatches[0].Value, categories[categoryMatches[0].Value]));
            }

            return Reply(new LocalEmbed()
                .WithAuthor(embedAuthor)
                .WithDescription(
                    "The following categories are available :"
                    + "```"
                    + string.Join("\n", categories.Select(category => $"* {category.Key}"))
                    + "```"
                    + $"\nCall {SelfInvocation()} `<category>` for commands belonging to a certain category."));
        }

        private LocalEmbed HandleCommand(Command command)
        {
            var prefixCategory = command.Attributes.OfType<PrefixCategoryAttribute>().FirstOrDefault()?.PrefixCategory;
            var config = Context.Services.GetRequiredService<IConfiguration>();
            var prefixes = string.IsNullOrWhiteSpace(prefixCategory)
                ? ((DefaultPrefixProvider) Context.Bot.Prefixes).Prefixes.Select(prefix => prefix.ToString()!).ToArray()
                : config.GetCommandPrefixes(prefixCategory);
            
            string basePrefix = prefixes[0], baseInvocation = $"{basePrefix}{command.Aliases[0]}";
            var otherPrefixes = prefixes.Length > 1 ? prefixes[1..] : System.Array.Empty<string>();
            var flagParams = command.Parameters
                .Where(param => param.Attributes.OfType<FlagAttribute>().Any())
                .Select(param => (param, param.Attributes.OfType<FlagAttribute>().First()))
                .ToList();

            return new LocalEmbed
            {
                Title = $"`{baseInvocation}`" + (
                    command.Aliases.Count > 1
                        ? $"({string.Join(", ", command.Aliases.Skip(1).Select(alias => $"`{basePrefix}{alias}`"))})"
                        : ""
                ),
                Description = string.IsNullOrWhiteSpace(command.Description) ? "No description." : command.Description,
                Fields = new List<LocalEmbedField?>
                {
                    new()
                    {
                        Name = "Syntax",
                        Value = $@"`{baseInvocation} {
                            string.Join(
                                ' ', 
                                command.Parameters.Select(param => {
                                    var flags = param.Attributes.OfType<FlagAttribute>().FirstOrDefault()?.Flags;
                                    // TODO we are assuming flag parameters are always optional
                                    var quotes = (flags == null ? "" : "f") + (param.IsOptional || flags != null ? "[]" : "<>");
                                    return param.IsMultiple
                                        ? quotes[..^1] + param.Name + "1" + quotes[^1] + "..." + quotes[..^1] + param.Name + "N" + quotes[^1]
                                        : quotes[..^1] + param.Name + quotes[^1];
                                }))
                            }".TrimEnd()
                            + "`"
                            + "\n\n"
                            + string.Join('\n', command.Parameters.Select(param => $"- `{param.Name}` {param.Description}"))
                    },
                    (flagParams.Count != 0
                        ? new LocalEmbedField
                        {
                            Name = "Flags",
                            Value = $@"The following parameter{
                                    StringUtilities.Plural(flagParams.Count)
                                } are flags & must be prefixed with certain strings, listed below."
                                + "\n"
                                + string.Join(
                                    '\n',
                                    flagParams.Select(pair =>
                                    {
                                        var (param, flagAttribute) = pair;
                                        return $"`{param.Name}` **:** {string.Join(" | ", flagAttribute.Flags.Select(f => $"`{f}`"))}";
                                    })
                                )
                        }
                        : null)
                }.Where(_ => _ != null).ToList(),
                Footer = otherPrefixes.Length != 0
                    ? new LocalEmbedFooter().WithText($"Also callable under prefix {string.Join("/", otherPrefixes.Select(_ => $"\"{_}\""))}")
                    : default
                
            };
        }
        
        private LocalEmbed HandleCategory(string category, IReadOnlyCollection<Command> commands)
        {
            return new()
            {
                Description = $"The following command{StringUtilities.Plural(commands.Count)} belong to the **{category}** category :",
                Fields = commands.Select(command =>
                {
                    var prefixCategory = command.Attributes.OfType<PrefixCategoryAttribute>().FirstOrDefault()?.PrefixCategory;
                    var config = Context.Services.GetRequiredService<IConfiguration>();
                    var prefixes = string.IsNullOrWhiteSpace(prefixCategory)
                        ? ((DefaultPrefixProvider) Context.Bot.Prefixes).Prefixes.Select(prefix => prefix.ToString())
                        : config.GetCommandPrefixes(prefixCategory);
                    var basePrefix = prefixes.First();
                    return new LocalEmbedField
                    {
                        Name = $@"`{basePrefix}{command.Aliases[0]}`{(
                            command.Aliases.Count > 1
                            ? $" ({string.Join(", ", command.Aliases.Skip(1).Select(_ => $"`{basePrefix}{_}`"))})"
                            : ""
                        )}",
                        Value = string.IsNullOrWhiteSpace(command.Description) ? "No description specified." : command.Description
                    };
                })
                    .ToList()
            };
        }
        
        private string SelfInvocation() => $"{Context.Prefix}{Context.Command.Aliases[0]}";
    }
}