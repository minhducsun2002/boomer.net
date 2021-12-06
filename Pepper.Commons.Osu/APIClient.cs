using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using OsuSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;
using APIScoreInfo = Pepper.Commons.Osu.API.APIScoreInfo;
using Color = System.Drawing.Color;

namespace Pepper.Commons.Osu
{
    public abstract class APIClient
    {
        private readonly FastConcurrentTLru<string, Color> userColorCache = new(200, TimeSpan.FromSeconds(30 * 60));
        protected readonly HttpClient HttpClient;
        protected APIClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public abstract Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo);

        public virtual async Task<Color> GetUserColor(APIUser user)
        {
            var key = $"osu-user-avatar-{user.Id}";
            if (userColorCache.TryGet(key, out var @return))
            {
                return @return;
            }

            var avatar = await HttpClient.GetByteArrayAsync(user.AvatarUrl);
            var image = Image.Load(avatar);
            image.Mutate(img => img.Quantize(new OctreeQuantizer(new QuantizerOptions { MaxColors = 2 }))
                .Resize(1, 1, KnownResamplers.Bicubic));
            var color = image[0, 0];
            var result = Color.FromArgb(color.R, color.G, color.B);
            userColorCache.AddOrUpdate(key, result);

            return result;
        }

        public abstract Task<WorkingBeatmap> GetBeatmap(int beatmapId);
        public abstract Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId);

        public abstract Task<APIScoreInfo> GetScore(long scoreId, RulesetInfo rulesetInfo);
        public abstract Task<APIScoreInfo[]> GetUserScores(int userId, ScoreType scoreType, RulesetInfo rulesetInfo, int count = 100, int offset = 0);

        public abstract Task<IReadOnlyList<Score>> GetLegacyBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo);
        public abstract Task<IReadOnlyList<Score>> GetLegacyUserRecentScores(int userId, RulesetInfo rulesetInfo, int limit = 50);
    }
}