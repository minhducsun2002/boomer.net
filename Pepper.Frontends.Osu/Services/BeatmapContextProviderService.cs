using Pepper.Commons.Structures;
using Serilog;

namespace Pepper.Frontends.Osu.Services
{
    public class BeatmapContextProviderService : Service
    {
        private readonly Dictionary<string, int> cache = new();
        private readonly ILogger log;

        public BeatmapContextProviderService(ILogger log) => this.log = log;

        public void SetBeatmap(string channelId, int beatmapId)
        {
            cache[channelId] = beatmapId;
            log.Debug("Setting beatmap of channel {0} to {1}", channelId, beatmapId);
        }

        public int? GetBeatmap(string channelId) => cache.TryGetValue(channelId, out var beatmapId) ? beatmapId : null;
    }
}