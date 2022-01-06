using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BitFaster.Caching.Lru;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;
using Color = System.Drawing.Color;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        private readonly FastConcurrentTLru<string, Color> userColorCache = new(200, TimeSpan.FromSeconds(30 * 60));

        public override async Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo)
        {
            var rulesetName = rulesetInfo.ShortName;
            var html = await HttpClient.GetStringAsync($"https://osu.ppy.sh/users/{HttpUtility.UrlPathEncode(username)}/{rulesetName}");

            var doc = new HtmlDocument(); doc.LoadHtml(html);
            var raw = doc.DocumentNode.Descendants("div").First(node => node.HasClass("js-react--profile-page"))!;
            var data = raw.GetAttributeValue("data-initial-data", "")?.Replace("&quot;", "\"");
            var parsed = JObject.Parse(data!);
            return parsed["user"]!.ToObject<APIUser>()!;
        }
    }
}