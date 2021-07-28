using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Skinning;

namespace Pepper.Structures.External.Osu
{
    public class WorkingBeatmap : osu.Game.Beatmaps.WorkingBeatmap
    {
        private readonly Beatmap beatmap;
        internal WorkingBeatmap(Beatmap beatmap, int? beatmapId = null) : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;

            // beatmap.BeatmapInfo.Ruleset = ;
            if (beatmapId.HasValue) beatmap.BeatmapInfo.OnlineBeatmapID = beatmapId;
        }
        
        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null!;
        protected override Track GetBeatmapTrack() => null!;
        protected override ISkin GetSkin() => null!;
        public override Stream GetStream(string storagePath) => null!;
    }
}