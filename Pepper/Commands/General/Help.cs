using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pepper.FuzzySearch;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Utilities;
using Qmmands;
using Qmmands.Text;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commmands.General
{
    public class Help : GeneralCommand
    {
        [TextCommand("help")]
        [Description("Everything starts here.")]
        public IDiscordCommandResult Exec(
            [Description("A command or a category to show respective help entry.")] string query = ""
            )
        {
            var self = Context.Bot.CurrentUser;
            var service = Context.GetCommandService();
            var commands = service.EnumerateTextCommands();
            var categories = commands
                .GroupBy(
                    command => command.CustomAttributes.OfType<CategoryAttribute>().FirstOrDefault()?.Category ?? "",
                    command => command
                )
                .ToDictionary(group => group.Key, group => group.ToArray());

            var embedAuthor = new LocalEmbedAuthor
            {
                Name = $"{self!.Tag}",
                IconUrl = self.GetAvatarUrl(CdnAssetFormat.Png, 1024)
            };

            if (!string.IsNullOrWhiteSpace(query))
            {
                var commandMatches = service.FindCommands(query.ToCharArray());
                if (commandMatches.Count != 0)
                {
                    var pages = HandleCommand(commandMatches.Select(match => match.Command).ToArray()).ToList();

                    if (pages.Count > 1)
                    {
                        return Menu(
                            new DefaultTextMenu(new PagedView(new ListPageProvider(pages.Select(embed => new Page().WithEmbeds(embed)))))
                        );
                    }

                    return Reply(pages[0]);
                }

                var categoryMatch = new Fuse<string>(
                        categories.Keys,
                        false,
                        new StringFuseField<string>(s => s))
                    .Search(query.ToLowerInvariant())
                    .First();

                if (categoryMatch.Score <= 0.6)
                {
                    return Reply(HandleCategory(categoryMatch.Element, categories[categoryMatch.Element]));
                }
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

        private List<LocalEmbed> HandleCommand(ITextCommand[] commands)
        {
            var prefixCategory = commands[0].CustomAttributes.OfType<PrefixCategoryAttribute>().FirstOrDefault()?.PrefixCategory;
            var config = Context.Services.GetRequiredService<IConfiguration>();
            var prefixes = string.IsNullOrWhiteSpace(prefixCategory)
                ? ((DefaultPrefixProvider) Context.Bot.Prefixes).Prefixes.Select(prefix => prefix.ToString()!).ToArray()
                : config.GetCommandPrefixes(prefixCategory);

            string basePrefix = prefixes[0], baseInvocation = $"{basePrefix}{commands[0].Aliases[0]}";
            var otherPrefixes = prefixes.Length > 1 ? prefixes[1..] : System.Array.Empty<string>();

            var embeds = commands.Select((command, index) =>
            {
                var overload = commands.Length > 1 ? $" (overload {index + 1})" : "";
                var flagParams = command.Parameters
                    .Where(param => param.CustomAttributes.OfType<FlagAttribute>().Any())
                    .Select(param => (param, param.CustomAttributes.OfType<FlagAttribute>().First()))
                    .ToList();

                var fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Syntax" + overload,
                        Value = $@"`{baseInvocation} {
                            string.Join(
                                ' ',
                                command.Parameters.Select(param => {
                                    var flags = param.CustomAttributes.OfType<FlagAttribute>().FirstOrDefault()?.Flags;
                                    // TODO we are assuming flag parameters are always optional
                                    var quotes = (flags == null ? "" : "f") + (param.GetTypeInformation().IsOptional || flags != null ? "[]" : "<>");
                                    return param.GetTypeInformation().IsEnumerable
                                        ? quotes[..^1] + param.Name + "1" + quotes[^1] + "..." + quotes[..^1] + param.Name + "N" + quotes[^1]
                                        : quotes[..^1] + param.Name + quotes[^1];
                                }))
                        }".TrimEnd()
                                + "`\n\n"
                                + string.Join('\n',
                                    command.Parameters.Select(param => $"- `{param.Name}` {param.Description}"))
                    }
                };

                if (flagParams.Count != 0)
                {
                    fields.Add(
                        new LocalEmbedField
                        {
                            Name = "Flags" + overload,
                            Value = $@"The following parameter{
                                StringUtilities.Plural(flagParams.Count)
                            } are flags & must be prefixed with certain strings, listed below."
                                    + "\n"
                                    + string.Join(
                                        '\n',
                                        flagParams.Select(pair =>
                                        {
                                            var (param, flagAttribute) = pair;
                                            return
                                                $"`{param.Name}` **:** {string.Join(" | ", flagAttribute.Flags.Select(f => $"`{f}`"))}";
                                        })
                                    )
                        });
                }

                var embed = new LocalEmbed
                {
                    Title = $"`{baseInvocation}`" + (
                        commands[0].Aliases.Count > 1
                            ? $"({string.Join(", ", commands[0].Aliases.Skip(1).Select(alias => $"`{basePrefix}{alias}`"))})"
                            : ""
                    ),
                    Description = string.IsNullOrWhiteSpace(commands[0].Description)
                        ? "No description."
                        : commands[0].Description,
                    Fields = fields,
                    Footer = otherPrefixes.Length != 0
                        ? new LocalEmbedFooter().WithText(
                            $"Also callable under prefix {string.Join("/", otherPrefixes.Select(_ => $"\"{_}\""))}")
                        : default
                };

                return embed;
            });


            return embeds.ToList();
        }

        private LocalEmbed HandleCategory(string category, IReadOnlyCollection<ITextCommand> commands)
        {
            var restricted = false;
            var embed = new LocalEmbed
            {
                Description = $"The following command{StringUtilities.Plural(commands.Count)} belong to the **{category}** category :",
                Fields = commands.Select(command =>
                {
                    var prefixes = command.GetPrefixes(Context.Bot);
                    var basePrefix = prefixes.First();

                    // TODO: Do actual permission checking
                    var locked = command.Checks.OfType<RequireBotOwnerAttribute>().Any();
                    restricted |= locked;

                    return new LocalEmbedField
                    {
                        Name = (locked ? "\u1f512" : "") + $@"`{basePrefix}{command.Aliases[0]}`{(
                            command.Aliases.Count > 1
                            ? $" ({string.Join(", ", command.Aliases.Skip(1).Select(_ => $"`{basePrefix}{_}`"))})"
                            : ""
                        )}",
                        Value = string.IsNullOrWhiteSpace(command.Description) ? "No description specified." : command.Description
                    };
                })
                    .ToList()
            };

            if (restricted)
            {
                embed.Footer = new LocalEmbedFooter().WithText("Certain commands cannot be invoked.");
            }

            return embed;
        }

        private string SelfInvocation() => $"{Context.Prefix}{Context.Command!.Aliases[0]}";
    }
}