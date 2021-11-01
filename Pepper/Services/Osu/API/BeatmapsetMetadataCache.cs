using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Pepper.Commons.Osu.API;

namespace Pepper.Services.Osu.API
{
    internal class BeatmapsetMetadataCache
    {
        private static readonly HttpClient HttpClient = new();
        private readonly FastConcurrentLru<string, APIBeatmapSet> cache = new(200);

        public async Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId)
        {
            var key = id + "-" + (isBeatmapSetId ? "1" : "0");
            if (cache.TryGet(key, out var @return))
            {
                return @return;
            }

            var res = await HttpClient.GetStringAsync(
                $"https://osu.ppy.sh/{(isBeatmapSetId ? "beatmapsets" : "beatmaps")}/{id}"
            );

            var doc = new HtmlDocument(); doc.LoadHtml(res);
            var obj = JsonConvert.DeserializeObject<APIBeatmapSet>(doc.GetElementbyId("json-beatmapset").InnerText)!;

            cache.AddOrUpdate(key, obj);
            return obj;
        }
    }
}