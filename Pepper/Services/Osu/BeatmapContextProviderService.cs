using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Pepper.Structures;
using Serilog;

namespace Pepper.Services.Osu
{
    public class BeatmapContextProviderService : Service
    {
        private static readonly HttpClient httpClient = new();
        private readonly string? keyValueUrl;
        private const string key = "beatmap-context";
        public BeatmapContextProviderService(IConfiguration config)
        {
            keyValueUrl = config.GetSection("kv-storage").Get<string[]>()?[0];
        }

        private Dictionary<string, int> cache = new();
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<BeatmapContextProviderService>();

        public void SetBeatmap(string channelId, int beatmapId)
        {
            cache[channelId] = beatmapId;
            Log.Debug("Setting beatmap of channel {0} to {1}", channelId, beatmapId);
        }

        public int? GetBeatmap(string channelId) => cache.TryGetValue(channelId, out var beatmapId) ? beatmapId : null;
        private string Serialize => string.Join("\n", cache.Select(kv => $"{kv.Key} => {kv.Value}"));

        private static Dictionary<string, int> Deserialize(string str)
            => str.Split("\n").Where(line => !string.IsNullOrEmpty(line))
                .Select(line => line.Split(" => "))
                .GroupBy(line => line[0])
                .ToDictionary(line => line.Key, line => int.Parse(line.First()[1]));
    }
}