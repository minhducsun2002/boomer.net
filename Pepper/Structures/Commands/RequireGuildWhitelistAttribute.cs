using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Database;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public class RequireGuildWhitelistAttribute : DiscordGuildCheckAttribute
    {
        public readonly string CommandIdentifier;
        public RequireGuildWhitelistAttribute(string commandIdentifier) => CommandIdentifier = commandIdentifier;

        public override async ValueTask<CheckResult> CheckAsync(DiscordGuildCommandContext context)
        {
            var allowed = await context.Services.GetRequiredService<RestrictedCommandWhitelistProvider>()
                .IsAllowedGuild(context.GuildId.ToString(), CommandIdentifier);
            return allowed
                ? Success()
                : Failure("This guild is not whitelisted to run this command.");
        }
    }
}