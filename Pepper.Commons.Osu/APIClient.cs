using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using Pepper.Commons.Osu.API;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;
using Color = System.Drawing.Color;

namespace Pepper.Commons.Osu
{
    public abstract class APIClient
    {
        private readonly FastConcurrentTLru<string, Color> userColorCache = new(200, TimeSpan.FromSeconds(30 * 60));
        internal static Ruleset[] BuiltInRulesets => SharedConstants.BuiltInRulesets;
        internal static Dictionary<int, ImmutableArray<Mod>> BuiltInMods => SharedConstants.BuiltInMods;

        protected readonly HttpClient HttpClient;
        protected APIClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public abstract Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo);
        public abstract Task<APIUser> GetUser(int id, RulesetInfo rulesetInfo);
        public abstract Task<APIUser> GetUserDefaultRuleset(string username);

        public async Task<Color> GetUserColor(APIUser user)
        {
            var key = $"osu-user-avatar-{user.Id}";
            if (userColorCache.TryGet(key, out var @return))
            {
                return @return;
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (user.AvatarUrl == null)
            {
                return Color.White;
            }

            var avatar = await HttpClient.GetByteArrayAsync(user.AvatarUrl);
            var image = Image.Load<Argb32>(avatar);
            image.Mutate(img => img.Quantize(new OctreeQuantizer(new QuantizerOptions { MaxColors = 2 }))
                .Resize(1, 1, KnownResamplers.Bicubic));
            var color = image[0, 0];
            var result = Color.FromArgb(color.R, color.G, color.B);
            userColorCache.AddOrUpdate(key, result);

            return result;
        }

        public abstract Task<WorkingBeatmap> GetBeatmap(int beatmapId);
        public abstract Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId);

        public abstract Task<APIScore> GetScore(long scoreId, RulesetInfo rulesetInfo);
        public abstract Task<APIScore[]> GetUserScores(int userId, ScoreType scoreType, RulesetInfo rulesetInfo, bool includeFails = false, int count = 100, int offset = 0);
        public abstract Task<APIScore[]> GetUserBeatmapScores(int userId, int beatmapId, RulesetInfo rulesetInfo);
    }
}