using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Enums;
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


        public (Difficulty, Song)? ResolveSongExact(int id, Commons.Maimai.Structures.Enums.Difficulty difficulty)
        {
            if (!songCache.TryGetValue(id, out var song))
            {
                return null;
            }

            var res = ResolveDiff(id, difficulty);
            return res != null ? (res, song) : null;
        }

        public (Difficulty, Song)? ResolveSongExact(string name, Commons.Maimai.Structures.Enums.Difficulty difficulty, (int, bool) level)
        {
            if (!nameCache.TryGetValue(name, out var ids))
            {
                return null;
            }

            foreach (var id in ids)
            {
                var res = ResolveDiff(id, difficulty);
                if (res != null)
                {
                    return (res, songCache[id]);
                }
            }

            return null;
        }

        private Difficulty? ResolveDiff(int id, Commons.Maimai.Structures.Enums.Difficulty difficulty, (int, bool)? level = null)
        {
            var diffs = songCache[id].Difficulties;
            foreach (var diff in diffs)
            {
                if ((int) difficulty == diff.Order)
                {
                    if (level == null)
                    {
                        return diff;
                    }
                    if (diff.Level == level.Value.Item1 && (diff.LevelDecimal >= 7 == level.Value.Item2))
                    {
                        return diff;
                    }
                }
            }

            return null;
        }

        public (Difficulty, Song)? ResolveSongLoosely(string name, Commons.Maimai.Structures.Enums.Difficulty difficulty, ChartVersion version)
        {
            if (!nameCache.TryGetValue(name, out var ids))
            {
                return null;
            }

            int id;
            if (ids.Count > 1)
            {
                id = version == ChartVersion.Deluxe
                    ? Math.Max(ids[0], ids[1])
                    : Math.Min(ids[0], ids[1]);
            }
            else
            {
                id = ids[0];
            }

            return ResolveSongExact(id, difficulty);
        }

        public async Task Load(MaimaiDataDbContext dataDb, HttpClient httpClient, CancellationToken stoppingToken)
        {
            Log.Information("Loading song data...");
            var difficulty = await dataDb.AddVersions.OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync(cancellationToken: stoppingToken);
            if (difficulty != null)
            {
                NewestVersion = difficulty.Id;
            }
            var songEntries = await dataDb.Songs
                .Include(s => s.Difficulties.Where(d => d.Enabled).OrderBy(d => d.Order))
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
            var s = await httpClient.GetStringAsync("https://maimai.sega.jp/data/maimai_songs.json", stoppingToken);
            var parsed = JsonConvert.DeserializeObject<SongImageData[]>(s);
            var mapped = parsed!
                .DistinctBy(s => s.Name)
                .ToDictionary(s => s.Name, s => s.ImageFileName);
            imageNameCache = mapped;
            Log.Information("Loaded image data");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dataDb = scope.ServiceProvider.GetRequiredService<MaimaiDataDbContext>();
                var httpClient = scope.ServiceProvider.GetRequiredService<HttpClient>();
                await Load(dataDb, httpClient, stoppingToken);
            }

            await base.ExecuteAsync(stoppingToken);
        }
    }
}