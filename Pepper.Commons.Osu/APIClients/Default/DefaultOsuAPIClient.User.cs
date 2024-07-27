using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;
using Polly;
using RestSharp;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        public override Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo)
            => InternalGetUser(username, null, rulesetInfo);

        public override Task<APIUser> GetUser(int userId, RulesetInfo rulesetInfo)
            => InternalGetUser(null, userId, rulesetInfo);

        public override Task<APIUser> GetUserDefaultRuleset(string username)
            => InternalGetUser(username, null);

        private async Task<APIUser> InternalGetUser(string? username, int? userId, RulesetInfo? rulesetInfo = null)
        {
            if (username == null && !userId.HasValue)
            {
                throw new ArgumentNullException(nameof(userId), "either an username or an user id must be passed!");
            }

            PolicyBuilder<APIUser> BasePolicy() => Policy.Handle<Exception>().OrResult((APIUser) null!);

            AsyncPolicy<APIUser> fallbackPolicy = BasePolicy()
                .FallbackAsync(async cancellationToken =>
                {
                    var type = userId.HasValue ? "id" : "username";
                    var encodedUser = HttpUtility.UrlEncode(userId.HasValue ? userId.Value.ToString() : username!);

                    return (await restClient.GetJsonAsync<APIUser>(
                        @$"users/{encodedUser}?key=${type}",
                        cancellationToken
                    ))!;
                });

            if (legacyClient != null && rulesetInfo != null)
            {
                fallbackPolicy = BasePolicy()
                    .FallbackAsync(async cancellationToken => userId.HasValue
                        ? await legacyClient.GetUserAsync(userId.Value, rulesetInfo, cancellationToken)
                        : await legacyClient.GetUserAsync(username!, rulesetInfo, cancellationToken))
                    .WrapAsync(fallbackPolicy);
            }


            var result = await fallbackPolicy
                .ExecuteAsync(
                    () => scrapingClient.GetUserAsync(username ?? userId!.Value.ToString(), rulesetInfo, CancellationToken.None)
                );

            return result;
        }
    }
}