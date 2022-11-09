using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Structures;
using Serilog;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;

namespace Pepper.Services.Maimai
{
    public class MaimaiDataService : Service
    {
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<MaimaiDataService>();
        private readonly IServiceProvider serviceProvider;
        private Dictionary<int, Song> songCache = new();
        private Dictionary<string, List<int>> nameCache = new();
        public int NewestVersion { get; private set; }

        public MaimaiDataService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public (Difficulty, Song)? ResolveSong(string name, Pepper.Commons.Maimai.Structures.Difficulty difficulty, (int, bool) level)
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dataDb = scope.ServiceProvider.GetRequiredService<MaimaiDataDbContext>();
                Log.Information("Loading song data...");
                var difficulty = await dataDb.AddVersions.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
                if (difficulty != null)
                {
                    NewestVersion = difficulty.Id;
                }
                var songEntries = await dataDb.Songs
                    .Include(s => s.Difficulties.Where(d => d.Enabled))
                    .ToListAsync(cancellationToken: stoppingToken);

                songCache = songEntries.ToDictionary(e => e.Id, e => e);

                nameCache = songEntries
                    .GroupBy(e => e.Name)
                    .ToDictionary(e => e.Key, e => e.Select(e => e.Id).ToList());
                Log.Information("Loaded {0} songs", songCache.Count);
            }
            
            await base.ExecuteAsync(stoppingToken);
        }
    }
}