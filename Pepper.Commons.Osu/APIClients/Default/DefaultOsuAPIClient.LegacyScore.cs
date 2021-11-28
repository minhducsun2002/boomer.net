using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Rulesets;
using OsuSharp;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        public Task<IReadOnlyList<Score>> GetLegacyBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo)
        {
            return legacyApiClient.GetScoresByBeatmapIdAndUserIdAsync(beatmapId, userId, (GameMode) rulesetInfo.ID!);
        }

        public Task<IReadOnlyList<Score>> GetLegacyUserRecentScores(int userId, RulesetInfo rulesetInfo, int limit = 50)
        {
            return legacyApiClient.GetUserRecentsByUserIdAsync(userId, (GameMode) rulesetInfo.ID!, limit);
        }
    }
}