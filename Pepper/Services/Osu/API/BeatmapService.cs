using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;
using Pepper.Structures.External.Osu;
using WorkingBeatmap = Pepper.Structures.External.Osu.WorkingBeatmap;

namespace Pepper.Services.Osu.API
{
    internal class BeatmapCache
    {
        private static readonly HttpClient HttpClient = new();
        private readonly FastConcurrentLru<long, byte[]> cache = new(200);

        public int CachedCount => cache.Count;
        
        private WorkingBeatmap decodeBeatmapFile(byte[] file)
        {
            using var stream = new MemoryStream(file);
            using var streamReader = new LineBufferedReader(stream);
            var workingBeatmap = new WorkingBeatmap(Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader));
            if (workingBeatmap.BeatmapInfo.Length.Equals(default))
                workingBeatmap.BeatmapInfo.Length = workingBeatmap.Beatmap.HitObjects[^1].StartTime;
            workingBeatmap.BeatmapInfo.Ruleset =
                RulesetTypeParser.SupportedRulesets[workingBeatmap.BeatmapInfo.RulesetID].RulesetInfo;

            return workingBeatmap;
        }
        
        public async Task<WorkingBeatmap> GetBeatmap(long beatmapId)
        {
            if (cache.TryGet(beatmapId, out var @return)) return decodeBeatmapFile(@return);

            var file = await HttpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{beatmapId}");

            cache.AddOrUpdate(beatmapId, file);
            
            return decodeBeatmapFile(file);
        }
    }
}