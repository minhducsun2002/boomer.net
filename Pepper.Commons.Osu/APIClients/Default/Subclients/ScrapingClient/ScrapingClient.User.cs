using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Osu.Exceptions;

namespace Pepper.Commons.Osu.APIClients.Default.Subclients
{
    internal partial class ScrapingClient
    {
        public Task<APIUser> GetUserAsync(string username, RulesetInfo? rulesetInfo, CancellationToken cancellationToken)
            => InternalGetUser(username, rulesetInfo, cancellationToken);
        public Task<APIUser> GetUserAsync(int userId, RulesetInfo? rulesetInfo, CancellationToken cancellationToken)
            => InternalGetUser(userId.ToString(), rulesetInfo, cancellationToken);

        private async Task<APIUser> InternalGetUser(string userIdentifier, RulesetInfo? rulesetInfo, CancellationToken cancellationToken)
        {
            var rulesetName = rulesetInfo?.ShortName ?? "";
            string html;
            try
            {
                var url = $"https://osu.ppy.sh/users/{HttpUtility.UrlPathEncode(userIdentifier)}/{rulesetName}";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                var res = await httpClient.SendAsync(request, cancellationToken);
                if (!res.IsSuccessStatusCode)
                {
                    throw new UserNotFoundException("User not found, status code was " + (int) res.StatusCode);
                }

                html = await res.Content.ReadAsStringAsync(cancellationToken);
            }
            catch (HttpRequestException httpRequestException)
            {
                throw new Exception("Internal error getting user data", httpRequestException);
            }

            var doc = new HtmlDocument(); doc.LoadHtml(html);
            var raw = doc.DocumentNode.Descendants("div").First(node => node.HasClass("js-react--profile-page"))!;
            var data = raw.GetAttributeValue("data-initial-data", "")?.Replace("&quot;", "\"");
            var parsed = JObject.Parse(data!);
            return parsed["user"]!.ToObject<APIUser>()!;
        }
    }
}