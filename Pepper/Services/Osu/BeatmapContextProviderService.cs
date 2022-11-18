using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pepper.Commons.Structures;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.Osu
{
    public class BeatmapContextProviderService : Service
    {
        private readonly Dictionary<string, int> cache = new();
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<BeatmapContextProviderService>();

        public void SetBeatmap(string channelId, int beatmapId)
        {
            cache[channelId] = beatmapId;
            Log.Debug("Setting beatmap of channel {0} to {1}", channelId, beatmapId);
        }

        public int? GetBeatmap(string channelId) => cache.TryGetValue(channelId, out var beatmapId) ? beatmapId : null;
    }
}