using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Database;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public class RequireGuildWhitelistAttribute : DiscordGuildCheckAttribute
    {
        public readonly string CommandIdentifier;
        public RequireGuildWhitelistAttribute(string commandIdentifier) => CommandIdentifier = commandIdentifier;

        public override async ValueTask<IResult> CheckAsync(IDiscordGuildCommandContext context)
        {
            var allowed = await context.Services.GetRequiredService<RestrictedCommandWhitelistProvider>()
                .IsAllowedGuild(context.GuildId.ToString(), CommandIdentifier);
            return allowed
                ? Results.Success
                : Results.Failure("This guild is not whitelisted to run this command.");
        }
    }
}