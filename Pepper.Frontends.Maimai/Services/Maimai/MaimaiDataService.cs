using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Structures;
using Pepper.Frontends.Maimai.Structures;
using Serilog;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;

namespace Pepper.Frontends.Maimai.Services
{
    public partial class MaimaiDataService : Service
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
        private Dictionary<string, DataOverlay[]> overlayCache = new();

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

        public (ChartLevel, ISong)? ResolveSongExact(
            string name, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty, (int, bool) level, ChartVersion chartVersion
        )
        {
            if (!nameCache.TryGetValue(name, out var ids))
            {
                return ResolveFromOverlay(name, difficulty, chartVersion);
            }

            // there are actually songs with identical names
            if (ids.Count == 2)
            {
                if (Math.Abs(ids[0] - ids[1]) == 10000)
                {
                    var id = chartVersion == ChartVersion.Deluxe ? ids.Max() : ids.Min();
                    ids = new List<int> { id };
                }
            }

            foreach (var id in ids)
            {
                var res = ResolveDiff(id, difficulty, level);
                if (res.HasValue)
                {
                    return (res.Value, SongCache[id]);
                }
            }

            return null;
        }

        private ChartLevel? ResolveDiff(int id, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty, (int, bool)? level = null)
        {
            var diffs = SongCache[id].Difficulties;
            foreach (var diff in diffs)
            {
                if ((int) difficulty == diff.Order)
                {
                    level = null;
                    if (level == null)
                    {
                        return diff.ExtractLevel();
                    }
                    if (diff.Level == level.Value.Item1)
                    {
                        if (diff.LevelDecimal >= 7 == level.Value.Item2 || diff.Level <= 6)
                        {
                            return diff.ExtractLevel();
                        }
                    }
                }
            }

            return null;
        }

        public (ChartLevel, ISong)? ResolveSongLoosely(string name, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty, ChartVersion version)
        {
            if (!nameCache.TryGetValue(name, out var ids))
            {
                return ResolveFromOverlay(name, difficulty, version);
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

            var res = ResolveDiff(id, difficulty);
            return res.HasValue ? (res.Value, SongCache[id]) : null;
        }

        private (ChartLevel, ISong)? ResolveFromOverlay(string name, Commons.Maimai.Structures.Data.Enums.Difficulty difficulty, ChartVersion version)
        {
            if (!overlayCache.TryGetValue(name, out var entries))
            {
                return null;
            }

            var res = entries.FirstOrDefault(d => d.Difficulty == difficulty && d.Version == version);
            if (res != null)
            {
                return (new ChartLevel { Whole = res.Level, Decimal = res.LevelDecimal }, res);
            }

            return null;
        }
    }
}