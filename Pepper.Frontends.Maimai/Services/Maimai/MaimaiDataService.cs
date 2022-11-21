using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Structures;
using Serilog;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;

namespace Pepper.Frontends.Maimai.Services
{
    public class MaimaiDataService : Service
    {
        private class SongImageData
        {
#pragma warning disable CS8618
            [JsonProperty("title")] public string Name { get; set; } = null!;
            [JsonProperty("image_url")] public string ImageFileName { get; set; } = null!;
#pragma warning restore CS8618
        }

        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<MaimaiDataService>();
        private readonly IServiceProvider serviceProvider;
        private Dictionary<string, string> imageNameCache = new();
        private Dictionary<int, Song> songCache = new();
        private Dictionary<string, List<int>> nameCache = new();
        public int NewestVersion { get; private set; }

        public MaimaiDataService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public bool HasMultipleVersions(string songName) => nameCache.TryGetValue(songName, out var ids) && ids.Count > 1;

        public string? GetImageUrl(string songName)
        {
            if (imageNameCache.TryGetValue(songName, out var fileName))
            {
                return "https://maimaidx-eng.com/maimai-mobile/img/Music/" + fileName;
            }

            return null;
        }

        public (Difficulty, Song)? ResolveSongExact(string name, Pepper.Commons.Maimai.Structures.Difficulty difficulty, (int, bool) level)
        {
            if (!nameCache.TryGetValue(name, out var ids))
            {
                return null;
            }

            foreach (var id in ids)
            {
                var diffs = songCache[id].Difficulties;
                foreach (var diff in diffs)
                {
                    if (diff.Level == level.Item1 &&
                        (diff.LevelDecimal >= 7 == level.Item2) &&
                        ((int) difficulty == diff.Order))
                    {
                        return (diff, songCache[id]);
                    }
                }
            }

            return null;
        }

        public (Difficulty, Song)? ResolveSongLoosely(string name, Pepper.Commons.Maimai.Structures.Difficulty difficulty, ChartVersion version)
        {
            if (!nameCache.TryGetValue(name, out var ids))
            {
                return null;
            }

            int id;
            if (ids.Count > 1)
            {
                id = Math.Max(ids[0], ids[1]);
            }
            else
            {
                id = ids[0];
            }
            var diffs = songCache[id].Difficulties;
            foreach (var diff in diffs)
            {
                if ((int) difficulty == diff.Order)
                {
                    return (diff, songCache[id]);
                }
            }

            return null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dataDb = scope.ServiceProvider.GetRequiredService<MaimaiDataDbContext>();
                Log.Information("Loading song data...");
                var difficulty = await dataDb.AddVersions.OrderByDescending(a => a.Id)
                    .FirstOrDefaultAsync(cancellationToken: stoppingToken);
                if (difficulty != null)
                {
                    NewestVersion = difficulty.Id;
                }
                var songEntries = await dataDb.Songs
                    .Include(s => s.Difficulties.Where(d => d.Enabled))
                    .Include(s => s.Artist)
                    .Include(s => s.Genre)
                    .Include(s => s.AddVersion)
                    .ToListAsync(cancellationToken: stoppingToken);

                songCache = songEntries.ToDictionary(e => e.Id, e => e);

                nameCache = songEntries
                    .GroupBy(e => e.Name)
                    .ToDictionary(e => e.Key, e => e.Select(e => e.Id).ToList());
                Log.Information("Loaded {0} songs", songCache.Count);

                Log.Information("Loading image data");
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                var s = await httpClient.GetStringAsync("https://maimai.sega.jp/data/maimai_songs.json", stoppingToken);
                var parsed = JsonConvert.DeserializeObject<SongImageData[]>(s);
                var mapped = parsed!
                    .DistinctBy(s => s.Name)
                    .ToDictionary(s => s.Name, s => s.ImageFileName);
                imageNameCache = mapped;
            }

            await base.ExecuteAsync(stoppingToken);
        }
    }
}