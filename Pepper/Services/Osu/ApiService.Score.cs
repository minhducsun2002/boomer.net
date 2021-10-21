using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using OsuSharp;
using Pepper.Services.Osu.API;
using User = osu.Game.Users.User;

namespace Pepper.Services.Osu
{
    public partial class APIService
    {
        public async Task<APILegacyScoreInfo[]> GetUserScores(int userId, ScoreType scoreType, RulesetInfo rulesetInfo, int count = 100, int offset = 0)
        {
            var scoreCache = new List<APILegacyScoreInfo>();

            var init = offset;
            while (scoreCache.Count < count)
            {
                const int maxSingle = 50;
                var res = await httpClient.GetStringAsync(
                    $"https://osu.ppy.sh/users/{userId}/scores/{scoreType.ToString().ToLowerInvariant()}?mode={rulesetInfo.ShortName}"
                    + $"&offset={init}&limit={Math.Min(maxSingle, count - init)}");
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

        public async Task<APILegacyScoreInfo> GetScore(long scoreId, RulesetInfo rulesetInfo)
        {
            var res = await httpClient.GetStringAsync($"https://osu.ppy.sh/scores/{rulesetInfo.ShortName}/{scoreId}");
            var doc = new HtmlDocument(); doc.LoadHtml(res);
            return SerializeToAPILegacyScoreInfo(JObject.Parse(doc.GetElementbyId("json-show").InnerText));
        }

        public async Task<IReadOnlyList<Score>> GetLegacyBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo)
        {
            return await legacyApiClient.GetScoresByBeatmapIdAndUserIdAsync(beatmapId, userId, (GameMode) rulesetInfo.ID!);
        }

        public async Task<IReadOnlyList<Score>> GetLegacyUserRecentScores(int userId, RulesetInfo rulesetInfo, int limit = 50)
        {
            return await legacyApiClient.GetUserRecentsByUserIdAsync(userId, (GameMode) rulesetInfo.ID!, limit);
        }

        private static APILegacyScoreInfo SerializeToAPILegacyScoreInfo(JToken scoreObject)
        {
            var score = scoreObject.ToObject<APILegacyScoreInfo>()!;
            var beatmap = scoreObject["beatmap"]!;
            score.Beatmap.BaseDifficulty = new BeatmapDifficulty
            {
                ApproachRate = beatmap["ar"]!.ToObject<float>(),
                CircleSize = beatmap["cs"]!.ToObject<float>(),
                DrainRate = beatmap["drain"]!.ToObject<float>(),
                OverallDifficulty = beatmap["accuracy"]!.ToObject<float>()
            };
            score.Beatmap.BPM = beatmap["bpm"]!.ToObject<double>();
            score.Beatmap.Length = beatmap["hit_length"]!.ToObject<double>() * 1000;
            return score;
        }
    }
}