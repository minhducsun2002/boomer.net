using Disqord;
using Disqord.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using osu.Game.Rulesets;
using Pepper.Commons.Interfaces.FailureFormattable;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Pepper.Frontends.Osu.Structures.TypeParsers;
using Qmmands;

namespace Pepper.Frontends.Osu.Structures
{
    public class FillUnknownRulesetAttribute : DiscordParameterCheckAttribute, IFailureFormattableParameterCheck
    {
        public override async ValueTask<IResult> CheckAsync(IDiscordCommandContext context, IParameter parameter, object? argument)
        {
            if (argument is not Ruleset ruleset)
            {
                return Results.Failure($"The argument is not of type {nameof(Ruleset)}");
            }

            if (ruleset is not UnknownRuleset)
            {
                return Results.Success;
            }

            var u = context.Arguments?.Values.OfType<Username>().FirstOrDefault();
            var gameServer = context.Arguments?.Values.OfType<GameServer>().FirstOrDefault();
            var username = u?.GetUsername(gameServer);

            if (gameServer == null)
            {
                return Results.Failure("No game server was determined. This is an internal error. Report this to the bot owner.");
            }

            if (username == null)
            {
                return await UsernameTypeParser.GetFailureMessageNoUsername(context);
            }

            APIUser user;
            try
            {
                var store = context.Services.GetRequiredService<APIClientStore>();
                var client = store.GetClient(gameServer.Value);
                user = await client.GetUserDefaultRuleset(username);
            }
            catch (Exception e)
            {
                return Results.Failure($"Failed to get user info: {e.Message}");
            }

            var favouriteMode = user.PlayMode;
            var resolvedRuleset = RulesetTypeParser.ResolveRuleset(favouriteMode.ToCharArray());
            ruleset = resolvedRuleset ?? RulesetTypeParser.SupportedRulesets.First();

            if (context.Arguments != null)
            {
                context.Arguments[parameter] = ruleset;
            }

            return Results.Success;
        }

        public bool Format(LocalMessageBase localMessageBase, IDiscordCommandContext context, IResult result)
        {
            localMessageBase.Content = result.FailureReason ?? "Failed to determine game mode. Report this to the bot owner.";
            return true;
        }

        public override bool CanCheck(IParameter parameter, object? value) =>
            typeof(Ruleset).IsAssignableFrom(parameter.GetTypeInformation().ActualType);
    }
}