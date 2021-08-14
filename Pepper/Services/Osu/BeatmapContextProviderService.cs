using System.Threading;
using System.Threading.Tasks;
using LazyCache;
using LazyCache.Providers;
using Microsoft.Extensions.Caching.Memory;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.Osu
{
    public class BeatmapContextProviderService : Service
    {
        private readonly IAppCache beatmapCache = new CachingService(
            new MemoryCacheProvider(new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 }))
        );
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<BeatmapContextProviderService>();

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            beatmapCache.CacheProvider.Dispose();
            return base.StopAsync(cancellationToken);
        }

        public void SetBeatmap(string channelId, int beatmapId)
        {
            beatmapCache.Add(channelId, beatmapId, new MemoryCacheEntryOptions { Size = 1 });
            Log.Debug("Setting beatmap of channel {0} to {1}", channelId, beatmapId);
        }

        public int? GetBeatmap(string channelId)
        {
            if (beatmapCache.TryGetValue<int>(channelId, out var beatmapId))
                return (int) beatmapId;
            return null;
        }
    }
}