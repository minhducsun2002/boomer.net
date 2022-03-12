using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    public partial class RippleOsuAPIClient
    {
        public override Task<APIScore> GetScore(long scoreId, RulesetInfo rulesetInfo)
        {
            throw new NotImplementedException();
        }

        public override async Task<APIScore[]> GetUserScores(
            int userId, ScoreType scoreType, RulesetInfo rulesetInfo, bool includeFails = false,
            int count = 100, int offset = 0)
        {
            if (count > 100)
            {
                throw new ArgumentOutOfRangeException($"{nameof(count)}", count, "Count must be not greater than 100!");
            }
            var path = scoreType switch
            {
                ScoreType.Best => "best",
                ScoreType.Recent => "recent",
                _ => throw new NotImplementedException("Getting user first-place scores is not implemented right now!")
            };

            int page = offset / count, rem = offset % count;

            IEnumerable<API.Ripple.Score> scores = new List<API.Ripple.Score>();
            var firstChunk = await HttpClient.GetStringAsync(
                $"https://ripple.moe/api/v1/users/scores/{path}?id={userId}&mode={rulesetInfo.OnlineID}&p={page + 1}&l={count}");

            scores = scores.Concat(JObject.Parse(firstChunk)["scores"]!.ToObject<API.Ripple.Score[]>()!);
            if (rem != 0)
            {
                var secondChunk = await HttpClient.GetStringAsync(
                    $"https://ripple.moe/api/v1/users/scores/{path}?id={userId}&mode={rulesetInfo.OnlineID}&p={page + 2}&l={count}");
                scores = scores.Concat(JObject.Parse(secondChunk)["scores"]!.ToObject<API.Ripple.Score[]>()!);
            }

            scores = scores.Skip(rem).Take(count).ToArray();

            var user = await GetUser(userId, rulesetInfo);
            var beatmapIds = scores.Select(s => s.Beatmap.Id).ToArray();
            var beatmaps = (await GetBulkBeatmapData(beatmapIds))
                .ToDictionary(m => m.OnlineID, m => m);

            var ret = scores.Select(score =>
                {
                    var mapInfo = beatmaps[score.Beatmap.Id];
                    var statistics = new Dictionary<string, int>
                    {
                        {"count_300", score.Count300},
                        {"count_100", score.Count100},
                        {"count_50", score.Count50},
                        {"count_geki", score.CountGeki},
                        {"count_katu", score.CountKatu},
                        {"count_miss", score.CountMiss},
                    };
                    return new APIScore
                    {
                        Beatmap = mapInfo,
                        BeatmapSet = mapInfo.BeatmapSet,
                        Mods = BuiltInRulesets[rulesetInfo.OnlineID].ConvertFromLegacyMods(score.Mods).Select(mod => new APIMod(mod)),
                        User = user,
                        Accuracy = score.Accuracy / 100,
                        Date = score.Time,
                        Perfect = score.FullCombo,
                        TotalScore = score.TotalScore,
                        MaxCombo = score.MaxCombo,
                        PP = score.PerformancePoints,
                        Statistics = statistics,
                        RulesetID = rulesetInfo.OnlineID,
                        OnlineID = score.Id,
                        Rank = score.Rank
                    };
                });

            return ret.ToArray();
        }

        public override async Task<APIScore[]> GetUserBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo)
        {
            throw new NotImplementedException();
        }
    }
}