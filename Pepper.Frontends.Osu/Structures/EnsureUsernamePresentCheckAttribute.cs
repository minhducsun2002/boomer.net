using Disqord;
using Disqord.Bot.Commands;
using Pepper.Commons.Interfaces.FailureFormattable;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Pepper.Frontends.Osu.Commands;
using Pepper.Frontends.Osu.Structures.Extensions;
using Qmmands;

namespace Pepper.Frontends.Osu.Structures
{
    public class EnsureUsernamePresentCheckAttribute : DiscordParameterCheckAttribute, IFailureFormattableParameterCheck
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

        public bool Format(LocalMessageBase localMessageBase, IDiscordCommandContext context, IResult _)
        {
            var saveCommand = context.GetCommandService()
                .EnumerateTextCommands()
                .FirstOrDefault(f => f.CustomAttributes.OfType<SaveUsernameAttribute>().Any());

            localMessageBase.Content = saveCommand != default
                ? $"You didn't save an username. Save one using `{saveCommand.GetPrimaryInvocation(context.Bot)}` your_username`."
                : "You didn't save an username.";
            return true;
        }

        public override bool CanCheck(IParameter parameter, object? value) =>
            typeof(Username).IsAssignableFrom(parameter.GetTypeInformation().ActualType);
    }
}