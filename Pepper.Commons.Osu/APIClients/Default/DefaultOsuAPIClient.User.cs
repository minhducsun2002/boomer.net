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
        public override async Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo)
        {
            PolicyBuilder<APIUser> BasePolicy() => Policy.Handle<Exception>().OrResult((APIUser) null!);

            AsyncPolicy<APIUser> fallbackPolicy = BasePolicy()
                .FallbackAsync(async cancellationToken =>
                    (await restClient.GetJsonAsync<APIUser>(
                        @$"users/{HttpUtility.UrlPathEncode(username)}",
                        cancellationToken
                    ))!
                );

            if (legacyClient != null)
            {
                fallbackPolicy = BasePolicy()
                    .FallbackAsync(async cancellationToken =>
                        await legacyClient.GetUserAsync(username, rulesetInfo, cancellationToken))
                    .WrapAsync(fallbackPolicy);
            }


            var result = await fallbackPolicy
                .ExecuteAsync(
                    () => scrapingClient.GetUserAsync(username, rulesetInfo, CancellationToken.None)
                );

            return result;
        }
    }
}