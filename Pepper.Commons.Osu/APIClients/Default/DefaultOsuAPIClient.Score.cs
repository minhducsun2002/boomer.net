using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;
using RestSharp;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        public override async Task<APIScore> GetScore(long scoreId, RulesetInfo rulesetInfo)
        {
            var res = await HttpClient.GetStringAsync($"https://osu.ppy.sh/scores/{rulesetInfo.ShortName}/{scoreId}");
            var doc = new HtmlDocument(); doc.LoadHtml(res);
            return DeserializeToAPILegacyScoreInfo(JObject.Parse(doc.GetElementbyId("json-show").InnerText));
        }

        public override async Task<APIScore[]> GetUserScores(
            int userId, ScoreType scoreType,
            RulesetInfo rulesetInfo, bool includeFails = false, int count = 100, int offset = 0
        )
        {
            // return await scrapingClient.GetUserScoresAsync(userId, scoreType, rulesetInfo, includeFails, count, offset);
            return (await restClient.GetJsonAsync<APIScore[]>(
                $"users/{userId}/scores/{scoreType.ToString().ToLowerInvariant()}?mode={rulesetInfo.ShortName}"
                + $"&include_fails={(includeFails ? 1 : 0)}&limit={count}&offset={offset}"
            ))!;
        }

        public override async Task<APIScore[]> GetUserBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo)
        {
            var response = await restClient.GetJsonAsync<APIScoreList>(
                $"/beatmaps/{beatmapId}/scores/users/{userId}/all"
            );
            return response!.Scores
                .Select(score =>
                {
                    score.Accuracy *= 100;
                    return score;
                })
                .ToArray();
        }

        internal static APIScore DeserializeToAPILegacyScoreInfo(JToken scoreObject)
        {
            var score = scoreObject.ToObject<APIScore>()!;
            var beatmap = scoreObject["beatmap"]!;
            score.Beatmap.BPM = beatmap["bpm"]!.ToObject<double>();
            score.Beatmap.Length = beatmap["hit_length"]!.ToObject<double>() * 1000;
            return score;
        }
    }
}