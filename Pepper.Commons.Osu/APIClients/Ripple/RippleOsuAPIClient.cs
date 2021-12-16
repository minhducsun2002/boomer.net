using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using OsuSharp;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;
using APIScoreInfo = Pepper.Commons.Osu.API.APIScoreInfo;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    [GameServer(GameServer.Ripple)]
    public partial class RippleOsuAPIClient : APIClient
    {
        private readonly OsuClient legacyApiClient;
        public RippleOsuAPIClient(HttpClient httpClient) : base(httpClient)
        {
            legacyApiClient = new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = "placeholderApiKey",
                BaseUrl = "https://ripple.moe/api"
            });
        }

        public override Task<WorkingBeatmap> GetBeatmap(int beatmapId)
        {
            throw new NotImplementedException();
        }

        public override Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId)
        {
            throw new NotImplementedException();
        }

        public override Task<APIScoreInfo> GetScore(long scoreId, RulesetInfo rulesetInfo)
        {
            throw new NotImplementedException();
        }

        public override Task<APIScoreInfo[]> GetUserScores(int userId, ScoreType scoreType, RulesetInfo rulesetInfo, int count = 100, int offset = 0)
        {
            // TODO : Do actual parsing
            return Task.FromResult(Array.Empty<APIScoreInfo>());
        }

        public override Task<IReadOnlyList<Score>> GetLegacyBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo)
        {
            throw new NotImplementedException();
        }

        public override Task<IReadOnlyList<Score>> GetLegacyUserRecentScores(int userId, RulesetInfo rulesetInfo, int limit = 50)
        {
            throw new NotImplementedException();
        }
    }
}