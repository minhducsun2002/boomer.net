using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services;
using Pepper.Structures.Commands;
using Qmmands;
using Command = Pepper.Structures.Command;

namespace Pepper.Commands.Reserved
{
    [RequireGuild]
    public class WhitelistCommand : Command
    {
        [Command("toggle-cmd")]
        [Description("Toggle the locking state of a command.")]
        public async Task<DiscordCommandResult> Exec(
            [Description("An alias of the command toggle.")] string alias,
            [Description("Guild ID to toggle. Default to the current guild.")] string guildId = "" 
        )
        {
            var context = (DiscordGuildCommandContext) Context;
            var searchResults = context.Bot.Commands.FindCommands(alias);
            if (searchResults.Count == 0) 
                return Reply("Sorry, couldn't found a matching command.");
            
            var command = searchResults[0].Command;
            var restrictionCheckAttribute = command.Checks.OfType<RequireGuildWhitelistAttribute>().FirstOrDefault();
            if (restrictionCheckAttribute == default)
                return Reply($"The command you specified, `{command.Aliases[0]}`, is not restricted.");

            var checkResult = await restrictionCheckAttribute.CheckAsync(context);
            var service = Context.Bot.Services.GetRequiredService<RestrictedCommandWhitelistService>();

            if (string.IsNullOrWhiteSpace(guildId)) guildId = Context.GuildId.ToString()!;
            if (checkResult.IsSuccessful)
            {
                service.RemoveAllowedGuild(restrictionCheckAttribute.CommandIdentifier, guildId);
                return Reply(
                    $"Preventing command `{command.Aliases[0]}` to be called from "
                    + (guildId == context.GuildId.ToString() ? "this guild." : $"guild ID {guildId}.")
                );
            }
            
            service.AddAllowedGuild(restrictionCheckAttribute.CommandIdentifier, guildId);
            return Reply(
                $"Allowed command `{command.Aliases[0]}` to be called from "
                + (guildId == context.GuildId.ToString() ? "this guild." : $"guild ID {guildId}.")
            );
        }
    }
}