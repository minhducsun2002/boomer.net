using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using FuzzySharp;
using Pepper.Structures.Commands;
using Pepper.Structures.Commands.Result;
using Pepper.Utilities;
using Qmmands;

namespace Pepper.Commmands.General
{
    public class Help : GeneralCommand
    {
        [Command("help")]
        [Description("Everything starts here.")]
        public async Task<EmbedResult> Exec(
            [Description("A command or a category to show respective help entry.")] string query = ""
            )
        {
            var self = Context.Client.CurrentUser;
            var service = Context.CommandService.QmmandService;
            var commands = service.GetAllCommands()!;
            var categories = commands
                .GroupBy(
                    command => command.Attributes.OfType<CategoryAttribute>().FirstOrDefault()?.Category ?? "",
                    command => command
                )
                .ToDictionary(group => group.Key, group => group.ToArray());

            var embedAuthor = new EmbedAuthorBuilder
            {
                Name = $"{self.Username}#{self.DiscriminatorValue}",
                IconUrl = self.GetAvatarUrl(ImageFormat.Png, 1024)
            };
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                var commandMatches = service.FindCommands(query);
                if (commandMatches.Count != 0) return new EmbedResult
                {
                    DefaultEmbed = HandleCommand(commandMatches[0].Command).Build()
                };
                
                var categoryMatches = Process.ExtractTop(
                    query.ToLowerInvariant(),
                    categories.Select(_ => _.Key),
                    s => s.ToLowerInvariant(),
                    limit: 1)
                    .ToArray();

                if (categoryMatches[0].Score >= 60)
                {
                    return new EmbedResult
                    {
                        DefaultEmbed = HandleCategory(categoryMatches[0].Value, categories[categoryMatches[0].Value])
                            .Build()
                    };
                }
            }
            
            return new EmbedResult
            {
                DefaultEmbed = new EmbedBuilder()
                    .WithAuthor(embedAuthor)
                    .WithDescription(
                        "The following categories are available :"
                        + "```"
                        + string.Join("\n", categories.Select(category => $"* {category.Key}"))
                        + "```"
                        + $"\nCall {SelfInvocation()} `<category>` for commands belonging to a certain category.")
                    .Build()
            };
        }

        private EmbedBuilder HandleCommand(Command command)
        {
            var prefixCategory = command.Attributes.OfType<PrefixCategoryAttribute>().FirstOrDefault()?.PrefixCategory;
            var service = Context.CommandService;
            var prefixes = string.IsNullOrWhiteSpace(prefixCategory)
                ? service.CategoriesByAllowedPrefixes.Keys.ToArray()
                : service.AllowedPrefixesByCategories[prefixCategory].ToArray();
            string basePrefix = prefixes[0], baseInvocation = $"{basePrefix}{command.Aliases[0]}";
            var otherPrefixes = prefixes.Length > 1 ? prefixes[1..] : new string[] {};
            var flagParams = command.Parameters
                .Where(param => param.Attributes.OfType<FlagAttribute>().Any())
                .Select(param => (param, param.Attributes.OfType<FlagAttribute>().First()))
                .ToList();
            
            return new EmbedBuilder
            {
                Title = $"`{baseInvocation}`" + (
                    command.Aliases.Count > 1
                        ? $"({string.Join(", ", command.Aliases.Skip(1).Select(alias => $"`{basePrefix}{alias}`"))})"
                        : ""
                ),
                Description = string.IsNullOrWhiteSpace(command.Description) ? "No description." : command.Description,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
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
                        ? new EmbedFieldBuilder
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
                        : null)!
                }.Where(_ => _ != null).ToList(),
                Footer = new EmbedFooterBuilder
                {
                    Text = otherPrefixes.Length != 0
                        ? $"Also callable under prefix {string.Join("/", otherPrefixes.Select(_ => $"\"{_}\""))}"
                        : ""
                }
            };
        }
        
        private EmbedBuilder HandleCategory(string category, IReadOnlyCollection<Command> commands)
        {
            return new EmbedBuilder
            {
                Description = $"The following command{StringUtilities.Plural(commands.Count)} belong to the **{category}** category :",
                Fields = commands.Select(command =>
                {
                    var prefixCategory = command.Attributes.OfType<PrefixCategoryAttribute>().FirstOrDefault()?.PrefixCategory;
                    var service = Context.CommandService;
                    var prefixes = string.IsNullOrWhiteSpace(prefixCategory)
                        ? service.CategoriesByAllowedPrefixes.Keys.ToArray()
                        : service.AllowedPrefixesByCategories[prefixCategory].ToArray();
                    var basePrefix = prefixes[0];
                    return new EmbedFieldBuilder
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
        
        
        
        private string SelfInvocation() =>
            $"{Context.CommandService.CategoriesByAllowedPrefixes.Keys.First()}{Context.Command.Aliases[0]}";
    }
}