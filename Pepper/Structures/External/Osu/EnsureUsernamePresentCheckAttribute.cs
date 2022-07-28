using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Structures.External.Osu.Extensions;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class EnsureUsernamePresentCheckAttribute : DiscordParameterCheckAttribute
    {
        public override ValueTask<IResult> CheckAsync(IDiscordCommandContext context, IParameter parameter, object? argument)
        {
            if (argument is Username username)
            {
                var server = context.Arguments!.Values.OfType<GameServer>().FirstOrDefault();
                if (username.GetUsername(server) == null)
                {
                    return Results.Failure(
                        $"Username for server {server.GetDisplayText()} not present for user {username.DiscordUserId}"
                    );
                }

                return Results.Success;
            }

            return Results.Failure($"The argument is not of type {nameof(Username)}");
        }

        public override bool CanCheck(IParameter parameter, object? value) =>
            typeof(Username).IsAssignableFrom(parameter.GetTypeInformation().ActualType);
    }
}