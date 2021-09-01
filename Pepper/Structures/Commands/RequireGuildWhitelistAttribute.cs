using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public class RequireGuildWhitelistAttribute : DiscordGuildCheckAttribute
    {
        public readonly string CommandIdentifier;
        public RequireGuildWhitelistAttribute(string commandIdentifier) => CommandIdentifier = commandIdentifier;

        public override ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            var allowedServers = context.Services.GetRequiredService<RestrictedCommandWhitelistService>()
                .GetAllowedGuilds(CommandIdentifier);
            return allowedServers.Contains(context.GuildId.ToString())
                ? Success()
                : Failure("This guild is not whitelisted to run this command.");
        }
    }
}