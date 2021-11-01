using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using WorkingBeatmap = Pepper.Commons.Osu.WorkingBeatmap;

namespace Pepper.Services.Osu.API
{
    internal class BeatmapCache
    {
        private static readonly HttpClient HttpClient = new();
        private readonly FastConcurrentLru<int, byte[]> cache = new(200);

        public int CachedCount => cache.Count;

        public async Task<WorkingBeatmap> GetBeatmap(int beatmapId)
        {
            if (cache.TryGet(beatmapId, out var @return))
            {
                return WorkingBeatmap.Decode(@return, beatmapId);
            }

            var file = await HttpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{beatmapId}");

            cache.AddOrUpdate(beatmapId, file);

            return WorkingBeatmap.Decode(file, beatmapId);
        }
    }
}