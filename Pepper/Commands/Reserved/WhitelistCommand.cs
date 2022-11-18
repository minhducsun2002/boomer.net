using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Database;
using Pepper.Services;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Checks;
using Pepper.Structures.CommandAttributes.Metadata;
using Qmmands;
using Qmmands.Text;
using Command = Pepper.Commons.Structures.Command;

namespace Pepper.Commands.Reserved
{
    [RequireGuild]
    public class WhitelistCommand : ReservedCommand
    {
        [TextCommand("toggle-cmd")]
        [Description("Toggle the locking state of a command.")]
        public async Task<IDiscordCommandResult> Exec(
            [Description("An alias of the command toggle.")] string alias,
            [Description("Guild ID to toggle. Default to the current guild.")] string guildId = ""
        )
        {
            var context = (IDiscordGuildCommandContext) Context;
            var searchResults = context.Bot.Commands.FindCommands(alias.ToCharArray());
            if (searchResults.Count == 0)
            {
                return Reply("Sorry, couldn't found a matching command.");
            }

            var command = searchResults[0].Command;
            var restrictionCheckAttribute = command.Checks.OfType<RequireGuildWhitelistAttribute>().FirstOrDefault();
            if (restrictionCheckAttribute == default)
            {
                return Reply($"The command you specified, `{command.Aliases[0]}`, is not restricted.");
            }

            var checkResult = await restrictionCheckAttribute.CheckAsync(context);
            var service = Context.Bot.Services.GetRequiredService<RestrictedCommandWhitelistProvider>();

            if (string.IsNullOrWhiteSpace(guildId))
            {
                guildId = Context.GuildId.ToString()!;
            }

            bool success;
            if (checkResult.IsSuccessful)
            {
                success = await service.RemoveAllowedGuild(guildId, restrictionCheckAttribute.CommandIdentifier);
                return Reply(
                     (success ? "Preventing" : "Failed to prevent")
                    + $" command `{command.Aliases[0]}` to be called from "
                    + (guildId == context.GuildId.ToString() ? "this guild" : $"guild ID {guildId}")
                    + (success ? "." : " : something was wrong syncing entries back to database.")
                );
            }

            success = await service.AddAllowedGuild(guildId, restrictionCheckAttribute.CommandIdentifier);
            return Reply(
                (success ? "Allowing" : "Failed to allow")
                + $" command `{command.Aliases[0]}` to be called from "
                + (guildId == context.GuildId.ToString() ? "this guild" : $"guild ID {guildId}")
                + (success ? "." : " : something was wrong syncing entries back to database.")
            );
        }
    }
}