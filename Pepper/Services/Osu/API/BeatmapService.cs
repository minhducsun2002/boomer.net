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
        private readonly FastConcurrentLru<long, WorkingBeatmap> cache = new(200);

        public async Task<WorkingBeatmap> GetBeatmap(long beatmapId)
        {
            if (cache.TryGet(beatmapId, out var @return)) return @return;

            await using var stream = await HttpClient.GetStreamAsync($"https://osu.ppy.sh/osu/{beatmapId}");
            using var streamReader = new LineBufferedReader(stream);
            var workingBeatmap = new WorkingBeatmap(Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader));
            if (workingBeatmap.BeatmapInfo.Length.Equals(default))
                workingBeatmap.BeatmapInfo.Length = workingBeatmap.Beatmap.HitObjects[^1].StartTime;
            workingBeatmap.BeatmapInfo.Ruleset =
                RulesetTypeParser.SupportedRulesets[workingBeatmap.BeatmapInfo.RulesetID].RulesetInfo;
            
            cache.AddOrUpdate(beatmapId, workingBeatmap);
            
            return workingBeatmap;

            
        }
    }
}