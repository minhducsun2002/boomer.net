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
            var workingBeatmap = new WorkingBeatmap(Decoder.GetDecoder<Beatmap>(streamReader).Decode(streamReader));
            if (workingBeatmap.BeatmapInfo.Length.Equals(default))
            {
                workingBeatmap.BeatmapInfo.Length = workingBeatmap.Beatmap.HitObjects[^1].StartTime;
            }

            workingBeatmap.BeatmapInfo.Ruleset = BuiltInRulesets[workingBeatmap.BeatmapInfo.RulesetID].RulesetInfo;
            workingBeatmap.BeatmapInfo.OnlineBeatmapID ??= beatmapId;

            return workingBeatmap;
        }
    }
}