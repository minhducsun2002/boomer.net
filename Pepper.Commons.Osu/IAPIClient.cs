using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using OsuSharp;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;
using APIScoreInfo = Pepper.Commons.Osu.API.APIScoreInfo;

namespace Pepper.Commons.Osu
{
    public interface IAPIClient
    {
        public Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo);
        public Task<Color> GetUserColor(APIUser userId);
        public Task<WorkingBeatmap> GetBeatmap(int beatmapId);
        public Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId);

        public Task<APIScoreInfo> GetScore(long scoreId, RulesetInfo rulesetInfo);
        public Task<APIScoreInfo[]> GetUserScores(int userId, ScoreType scoreType, RulesetInfo rulesetInfo, int count = 100, int offset = 0);

        public Task<IReadOnlyList<Score>> GetLegacyBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo);
        public Task<IReadOnlyList<Score>> GetLegacyUserRecentScores(int userId, RulesetInfo rulesetInfo, int limit = 50);
    }
}