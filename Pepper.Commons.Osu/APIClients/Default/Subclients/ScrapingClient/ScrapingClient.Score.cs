using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;

namespace Pepper.Commons.Osu.APIClients.Default.Subclients
{
    internal partial class ScrapingClient
    {
        public async Task<APIScore[]> GetUserScoresAsync(
            int userId, ScoreType scoreType,
            RulesetInfo rulesetInfo, bool includeFails = false, int count = 100, int offset = 0
        )
        {
            var scoreCache = new List<APIScore>();

            var init = offset;
            while (scoreCache.Count < count)
            {
                const int maxSingle = 50;
                var res = await httpClient.GetStringAsync(
                    $"https://osu.ppy.sh/users/{userId}/scores/{scoreType.ToString().ToLowerInvariant()}?mode={rulesetInfo.ShortName}"
                    + $"&offset={init}&limit={Math.Min(maxSingle, count - init)}&include_fails={(includeFails ? 1 : 0)}");
                init += maxSingle;
                var scores = JArray.Parse(res).Select(DefaultOsuAPIClient.DeserializeToAPILegacyScoreInfo).ToArray();
                scoreCache.AddRange(scores);
                if (scores.Length == 0)
                {
                    break;
                }
            }

            return scoreCache.Count > count ? scoreCache.GetRange(0, count).ToArray() : scoreCache.ToArray();
        }
    }
}