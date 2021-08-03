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
        private static readonly HttpClient HttpClient = new();
        private readonly IAppCache beatmapCache = new CachingService
        (
            new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions { SizeLimit = 50 }))
        );

        public async Task<WorkingBeatmap> GetBeatmap(long beatmapId)
        {
            async Task<WorkingBeatmap> BeatmapGetter()
            {
                await using var stream = await HttpClient.GetStreamAsync($"https://osu.ppy.sh/osu/{beatmapId}");
                using var streamReader = new LineBufferedReader(stream);
                var workingBeatmap = new WorkingBeatmap(Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader));
                if (workingBeatmap.BeatmapInfo.Length.Equals(default))
                    workingBeatmap.BeatmapInfo.Length = workingBeatmap.Beatmap.HitObjects[^1].StartTime;
                return workingBeatmap;
            }

            return await beatmapCache.GetOrAddAsync($"beatmap-{beatmapId}", BeatmapGetter, new MemoryCacheEntryOptions
            {
                Size = 1
            });
        }
    }
}