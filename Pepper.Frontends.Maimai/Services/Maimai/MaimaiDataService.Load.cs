using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pepper.Commons.Maimai;

namespace Pepper.Frontends.Maimai.Services
{
    public partial class MaimaiDataService
    {
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

            log?.Information("Loading overlay data");
            var overlay = (await dataDb.DataOverlays.ToListAsync(cancellationToken: stoppingToken))
                .GroupBy(d => d.Name)
                .ToDictionary(d => d.Key, d => d.ToArray());
            overlayCache = overlay;
            log?.Information("Loaded overlay data");
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