using System.Net.Http;
using System.Threading.Tasks;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using Pepper.Structures;
using WorkingBeatmap = Pepper.Structures.External.Osu.WorkingBeatmap;

namespace Pepper.Services.Osu.API
{
    internal class BeatmapCache
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly IAppCache beatmapCache = new CachingService
        (
            new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions { SizeLimit = 50 }))
        );

        public async Task<WorkingBeatmap> GetBeatmap(int beatmapId)
        {
            async Task<WorkingBeatmap> BeatmapGetter()
            {
                await using var stream = await httpClient.GetStreamAsync($"https://osu.ppy.sh/osu/{beatmapId}");
                using var streamReader = new LineBufferedReader(stream);
                return new WorkingBeatmap(Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader));
            }

            return await beatmapCache.GetOrAddAsync($"beatmap-{beatmapId}", BeatmapGetter, new MemoryCacheEntryOptions
            {
                Size = 1
            });
        }
    }
}