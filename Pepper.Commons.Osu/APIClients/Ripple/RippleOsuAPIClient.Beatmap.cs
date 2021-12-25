using System;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Osu.APIClients.Default;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    public partial class RippleOsuAPIClient
    {
        private readonly FastConcurrentLru<string, APIBeatmapSet> beatmapsetMetadataCache = new(200);
        public override Task<WorkingBeatmap> GetBeatmap(int beatmapId)
        {
            // just use the default client instead wtf why would you need this call?
            throw new NotImplementedException();
        }

        public override async Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId)
        {
            var key = GenerateKey(id, isBeatmapSetId);
            if (beatmapsetMetadataCache.TryGet(key, out var @return))
            {
                return @return;
            }

            var obj = await DefaultOsuAPIClient.GetFreshBeatmapsetInfo(HttpClient, id, isBeatmapSetId);

            beatmapsetMetadataCache.AddOrUpdate(key, obj);
            return obj;
        }

        private static string GenerateKey(long id, bool isBeatmapSetId) => id + "-" + (isBeatmapSetId ? "1" : "0");
    }
}