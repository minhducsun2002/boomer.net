using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using Newtonsoft.Json.Linq;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Osu.APIClients.Default;
using RestSharp;

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

        private async Task<BeatmapCompact[]> GetBulkBeatmapData(params int[] beatmapIds)
        {
            var output = new List<BeatmapCompact>();
            var mapids = beatmapIds.Distinct().ToArray();

            for (var i = 0; i < mapids.Length; i += 50)
            {
                var ids = mapids[i..Math.Min(i + 50, mapids.Length)];
                var response = await restClient.GetJsonAsync<APIBeatmapList>(
                    $"beatmaps?{string.Join('&', ids.Select(id => $"ids[]={id}"))}"
                );
                output.AddRange(response!.Beatmaps);
            }

            return output.ToArray();
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