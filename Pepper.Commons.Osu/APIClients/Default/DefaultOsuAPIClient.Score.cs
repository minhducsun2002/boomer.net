using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        public override async Task<APIScoreInfo> GetScore(long scoreId, RulesetInfo rulesetInfo)
        {
            var res = await HttpClient.GetStringAsync($"https://osu.ppy.sh/scores/{rulesetInfo.ShortName}/{scoreId}");
            var doc = new HtmlDocument(); doc.LoadHtml(res);
            return SerializeToAPILegacyScoreInfo(JObject.Parse(doc.GetElementbyId("json-show").InnerText));
        }

        public override async Task<APIScoreInfo[]> GetUserScores(
            int userId, ScoreType scoreType,
            RulesetInfo rulesetInfo, bool includeFails = false, int count = 100, int offset = 0
        )
        {
            var scoreCache = new List<APIScoreInfo>();

            var init = offset;
            while (scoreCache.Count < count)
            {
                const int maxSingle = 50;
                var res = await HttpClient.GetStringAsync(
                    $"https://osu.ppy.sh/users/{userId}/scores/{scoreType.ToString().ToLowerInvariant()}?mode={rulesetInfo.ShortName}"
                    + $"&offset={init}&limit={Math.Min(maxSingle, count - init)}&include_fails={(includeFails ? 1 : 0)}");
                init += maxSingle;
                var scores = JArray.Parse(res).Select(SerializeToAPILegacyScoreInfo).ToArray();
                scoreCache.AddRange(scores);
                if (scores.Length == 0)
                {
                    break;
                }
            }

            return scoreCache.Count > count ? scoreCache.GetRange(0, count).ToArray() : scoreCache.ToArray();
        }

        private static APIScoreInfo SerializeToAPILegacyScoreInfo(JToken scoreObject)
        {
            var score = scoreObject.ToObject<APIScoreInfo>()!;
            var beatmap = scoreObject["beatmap"]!;
            score.Beatmap.BPM = beatmap["bpm"]!.ToObject<double>();
            score.Beatmap.Length = beatmap["hit_length"]!.ToObject<double>() * 1000;
            return score;
        }
    }
}