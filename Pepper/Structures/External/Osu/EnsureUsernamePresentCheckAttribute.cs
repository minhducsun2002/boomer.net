using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Services;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu.Extensions;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class EnsureUsernamePresentCheckAttribute : DiscordParameterCheckAttribute
    {
        public class FailureFormatterAttribute : Attribute, IParameterCheckFailureFormatter
        {
            public LocalMessage? FormatFailure(
                ParameterChecksFailedResult parameterChecksFailedResult,
                DiscordCommandContext commandContext)
            {
                var server = commandContext.Services
                    .GetRequiredService<TypeParsedArgumentPersistenceService>()
                    .Get<GameServer>();
                return new LocalMessage().WithContent($"You don't have a saved username for the {server.GetDisplayText()} server.");
            }
        }

        public override async ValueTask<CheckResult> CheckAsync(object argument, DiscordCommandContext context)
        {
            if (argument is Username username)
            {
                var server = context.Services.GetRequiredService<TypeParsedArgumentPersistenceService>().Get<GameServer>();
                if (username.GetUsername(server) == null)
                {
                    return Failure(
                        $"Username for server {server.GetDisplayText()} not present for user {username.DiscordUserId}"
                    );
                }

                return Success();
            }

            return Failure($"The argument is not of type {nameof(Username)}");
        }
    }
}