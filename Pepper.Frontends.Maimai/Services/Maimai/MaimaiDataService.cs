using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Enums;
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

        private readonly ILogger? log;
        private readonly IServiceProvider serviceProvider;
        private Dictionary<string, string> imageNameCache = new();
        public Dictionary<int, Song> SongCache = new();
        private Dictionary<string, List<int>> nameCache = new();
        public Dictionary<int, string> GenreCache = new();

        public int NewestVersion { get; private set; }

        public MaimaiDataService(IServiceProvider serviceProvider, ILogger? logger = null)
        {
            this.serviceProvider = serviceProvider;
            log = logger?.ForContext<MaimaiDataService>();
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


        public (Difficulty, Song)? ResolveSongExact(int id, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty)
        {
            if (!SongCache.TryGetValue(id, out var song))
            {
                return null;
            }

            var res = ResolveDiff(id, difficulty);
            return res != null ? (res, song) : null;
        }

        public (Difficulty, Song)? ResolveSongExact(
            string name, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty, (int, bool) level, ChartVersion? chartVersion = null
        )
        {
            if (!nameCache.TryGetValue(name, out var ids))
            {
                return null;
            }

            if (chartVersion != null)
            {
                // there are actually songs with identical names
                if (ids.Count == 2)
                {
                    if (Math.Abs(ids[0] - ids[1]) == 10000)
                    {
                        var id = chartVersion == ChartVersion.Deluxe ? ids.Max() : ids.Min();
                        ids = new List<int> { id };
                    }
                }
            }

            foreach (var id in ids)
            {
                var res = ResolveDiff(id, difficulty, level);
                if (res != null)
                {
                    return (res, SongCache[id]);
                }
            }

            return null;
        }

        private Difficulty? ResolveDiff(int id, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty, (int, bool)? level = null)
        {
            var diffs = SongCache[id].Difficulties;
            foreach (var diff in diffs)
            {
                if ((int) difficulty == diff.Order)
                {
                    if (level == null)
                    {
                        return diff;
                    }
                    if (diff.Level == level.Value.Item1)
                    {
                        if (diff.LevelDecimal >= 7 == level.Value.Item2 || diff.Level <= 6)
                        {
                            return diff;
                        }
                    }
                }
            }

            return null;
        }

        public (Difficulty, Song)? ResolveSongLoosely(string name, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty, ChartVersion version)
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
            log?.Information("Loading song data...");
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

            SongCache = songEntries.ToDictionary(e => e.Id, e => e);

            nameCache = songEntries
                .GroupBy(e => e.Name)
                .ToDictionary(e => e.Key, e => e.Select(e => e.Id).ToList());
            log?.Information("Loaded {0} songs", SongCache.Count);

            log?.Information("Loading image data");
            var s = await httpClient.GetStringAsync("https://maimai.sega.jp/data/maimai_songs.json", stoppingToken);
            var parsed = JsonConvert.DeserializeObject<SongImageData[]>(s);
            var mapped = parsed!
                .DistinctBy(s => s.Name)
                .ToDictionary(s => s.Name, s => s.ImageFileName);
            imageNameCache = mapped;
            log?.Information("Loaded image data");

            log?.Information("Loading genre data");
            var categoryEntries = await dataDb.Genres.ToListAsync(cancellationToken: stoppingToken);
            GenreCache = categoryEntries.ToDictionary(g => g.Id, g => g.Name);
            log?.Information("Loaded {0} genres", GenreCache.Count);
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