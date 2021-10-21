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

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            if (keyValueUrl != null)
            {
                Log.Information("Restoring beatmap context cache...");
                try
                {
                    var text = await httpClient.GetStringAsync($"{keyValueUrl}/Data/Get/{key}", cancellationToken);
                    var dict = Deserialize(text);
                    cache = dict;
                    Log.Information("Restored {0} entries.", dict.Count);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Failed to load beatmap context cache. An empty cache will be used.");
                }
            }
            else
            {
                Log.Warning("A key-value store was not configured - no beatmap context synchronization will be carried out.");
            }

            await base.StartAsync(cancellationToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (keyValueUrl != null)
            {
                Log.Information("Synchronizing beatmap context cache...");
                try
                {
                    await httpClient.PostAsync($"{keyValueUrl}/Data/Push/{key}", new StringContent(Serialize), cancellationToken);
                    Log.Information("Synchronization complete.");
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Failed to sync beatmap context cache.");
                }
            }
            else
            {
                Log.Warning("A key-value store was not configured - not synchronizing cache back up.");
            }

            await base.StopAsync(cancellationToken);
        }
    }
}