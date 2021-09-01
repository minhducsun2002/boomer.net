using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public class RequireGuildWhitelistAttribute : DiscordGuildCheckAttribute
    {
        private readonly string uniqueCommandIdentifier;
        public RequireGuildWhitelistAttribute(string uniqueCommandIdentifier) => this.uniqueCommandIdentifier = uniqueCommandIdentifier;

        public override ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            var allowedServers = context.Services.GetRequiredService<RestrictedCommandWhitelistService>()
                .GetAllowedGuilds(uniqueCommandIdentifier);
            return allowedServers.Contains(context.GuildId.ToString())
                ? Success()
                : Failure("This guild is not whitelisted to run this command.");
        }
    }
}