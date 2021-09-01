using BitFaster.Caching.Lru;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.Osu
{
    public class BeatmapContextProviderService : Service
    {
        private readonly FastConcurrentLru<string, int> cache = new(500);
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<BeatmapContextProviderService>();

        public void SetBeatmap(string channelId, int beatmapId)
        {
            cache.AddOrUpdate(channelId, beatmapId);
            Log.Debug("Setting beatmap of channel {0} to {1}", channelId, beatmapId);
        }

        public int? GetBeatmap(string channelId) => cache.TryGet(channelId, out var beatmapId) ? beatmapId : null;
    }
}