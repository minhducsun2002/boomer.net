using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring.Legacy;
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
                    var mockScore = new SoloScoreInfo
                    {
                        RulesetID = rulesetInfo.OnlineID
                    }.ToScoreInfo(Array.Empty<Mod>());
                    mockScore.SetCount50(score.Count50);
                    mockScore.SetCount100(score.Count100);
                    mockScore.SetCount300(score.Count300);
                    mockScore.SetCountGeki(score.CountGeki);
                    mockScore.SetCountKatu(score.CountKatu);
                    mockScore.SetCountMiss(score.CountMiss);

                    return new APIScore
                    {
                        Beatmap = mapInfo,
                        BeatmapSet = mapInfo.BeatmapSet,
                        Mods = BuiltInRulesets[rulesetInfo.OnlineID].ConvertFromLegacyMods(score.Mods).Select(mod => new APIMod(mod)).ToArray(),
                        User = user,
                        Accuracy = score.Accuracy / 100,
                        EndedAt = score.Time,
                        Perfect = score.FullCombo,
                        TotalScore = score.TotalScore,
                        MaxCombo = score.MaxCombo,
                        PP = score.PerformancePoints,
                        Statistics = mockScore.Statistics,
                        RulesetID = rulesetInfo.OnlineID,
                        ID = (ulong) score.Id,
                        Rank = score.Rank
                    };
                });

            return ret.ToArray();
        }

        public override async Task<APIScore[]> GetUserBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo)
        {
            var scores = await legacyClient.GetScoresByBeatmapIdAndUserIdAsync(
                beatmapId,
                userId,
                (OsuSharp.GameMode) rulesetInfo.OnlineID
            );

            var user = await GetUser(userId, rulesetInfo);

            var beatmapIds = scores.Select(score => (int) score.BeatmapId).Distinct();
            var beatmaps = (await GetBulkBeatmapData(beatmapIds.ToArray()))
                .ToDictionary(map => map.OnlineID, map => map);

            return scores.Select(score =>
                {
                    var mapInfo = beatmaps[(int) score.BeatmapId];
                    var mockScore = new SoloScoreInfo
                    {
                        RulesetID = rulesetInfo.OnlineID
                    }.ToScoreInfo(Array.Empty<Mod>());
                    mockScore.SetCount50(score.Count50);
                    mockScore.SetCount100(score.Count100);
                    mockScore.SetCount300(score.Count300);
                    mockScore.SetCountGeki(score.Geki);
                    mockScore.SetCountKatu(score.Katu);
                    mockScore.SetCountMiss(score.Miss);

                    return new APIScore
                    {
                        Beatmap = mapInfo,
                        BeatmapSet = mapInfo.BeatmapSet,
                        Mods = BuiltInRulesets[rulesetInfo.OnlineID].ConvertFromLegacyMods((LegacyMods) score.Mods).Select(mod => new APIMod(mod)).ToArray(),
                        User = user,
                        Accuracy = score.Accuracy / 100,
                        EndedAt = score.Date!.Value,
                        Perfect = score.Perfect,
                        TotalScore = (int) score.TotalScore,
                        MaxCombo = score.MaxCombo!.Value,
                        PP = score.PerformancePoints,
                        Statistics = mockScore.Statistics,
                        RulesetID = rulesetInfo.OnlineID,
                        ID = (ulong?) score.ScoreId,
                        Rank = Enum.Parse<ScoreRank>(score.Rank)
                    };
                })
                .ToArray();
        }
    }
}