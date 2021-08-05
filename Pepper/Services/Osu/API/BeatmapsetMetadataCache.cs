using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace Pepper.Services.Osu.API
{
    internal class BeatmapsetMetadataCache
    {
        private static readonly HttpClient HttpClient = new();
        private readonly IAppCache beatmapsetMetaCache = new CachingService
        (
            new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 }))
        );

        public async Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId)
        {
            async Task<APIBeatmapSet> BeatmapsetGetter()
            {
                var res = await HttpClient.GetStringAsync(
                    $"https://osu.ppy.sh/{(isBeatmapSetId ? "beatmapsets" : "beatmaps")}/{id}"
                );

                var doc = new HtmlDocument(); doc.LoadHtml(res);
                return JsonConvert.DeserializeObject<APIBeatmapSet>(doc.GetElementbyId("json-beatmapset").InnerText)!;
            }

            return await beatmapsetMetaCache.GetOrAdd($"beatmap-{(isBeatmapSetId ? 0 : 1)}-{id}", BeatmapsetGetter, new LazyCacheEntryOptions { Size = 1 });
        }
    }
}