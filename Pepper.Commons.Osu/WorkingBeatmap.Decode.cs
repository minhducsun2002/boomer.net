using System.IO;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO;

namespace Pepper.Commons.Osu
{
    public partial class WorkingBeatmap
    {
        public static WorkingBeatmap Decode(byte[] beatmap, int? beatmapId = null)
        {
            using var stream = new MemoryStream(beatmap);
            using var streamReader = new LineBufferedReader(stream);
            Decoder.RegisterDependencies(new RulesetStore());
            var workingBeatmap = new WorkingBeatmap(Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader));
            if (workingBeatmap.BeatmapInfo.Length.Equals(default))
            {
                workingBeatmap.BeatmapInfo.Length = workingBeatmap.Beatmap.HitObjects[^1].StartTime;
            }

            workingBeatmap.BeatmapInfo.Ruleset = BuiltInRulesets[workingBeatmap.BeatmapInfo.Ruleset.OnlineID].RulesetInfo;
            if (beatmapId.HasValue)
            {
                workingBeatmap.BeatmapInfo.OnlineID = beatmapId.Value;
            }

            return workingBeatmap;
        }
    }
}