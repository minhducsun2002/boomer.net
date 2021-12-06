using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Pepper.Commons.Osu.API;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        private readonly FastConcurrentLru<int, byte[]> beatmapCache = new(200);
        private readonly FastConcurrentLru<string, APIBeatmapSet> beatmapsetMetadataCache = new(200);
        public override async Task<WorkingBeatmap> GetBeatmap(int beatmapId)
        {
            if (beatmapCache.TryGet(beatmapId, out var @return))
            {
                return WorkingBeatmap.Decode(@return, beatmapId);
            }

            var file = await HttpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{beatmapId}");
            beatmapCache.AddOrUpdate(beatmapId, file);
            return WorkingBeatmap.Decode(file, beatmapId);
        }

        public override async Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId)
        {
            var key = id + "-" + (isBeatmapSetId ? "1" : "0");
            if (beatmapsetMetadataCache.TryGet(key, out var @return))
            {
                return @return;
            }

            var res = await HttpClient.GetStringAsync(
                $"https://osu.ppy.sh/{(isBeatmapSetId ? "beatmapsets" : "beatmaps")}/{id}"
            );

            var doc = new HtmlDocument(); doc.LoadHtml(res);
            var obj = JsonConvert.DeserializeObject<APIBeatmapSet>(doc.GetElementbyId("json-beatmapset").InnerText)!;

            beatmapsetMetadataCache.AddOrUpdate(key, obj);
            return obj;
        }
    }
}