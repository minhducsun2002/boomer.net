using System;
using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace Pepper.Commons.Osu
{
    public partial class WorkingBeatmap : osu.Game.Beatmaps.WorkingBeatmap
    {
        private readonly DifficultyCalculator difficultyCalculator;
        private readonly Beatmap beatmap;
        private static readonly Ruleset[] BuiltInRulesets =
        {
            new OsuRuleset(),
            new TaikoRuleset(),
            new CatchRuleset(),
            new ManiaRuleset()
        };

        private WorkingBeatmap(Beatmap beatmap, int? beatmapId = null) : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;
            this.beatmap.BeatmapInfo.Ruleset = GetDefaultRuleset().RulesetInfo;
            difficultyCalculator = GetDefaultRuleset().CreateDifficultyCalculator(this);

            if (beatmapId.HasValue)
            {
                beatmap.BeatmapInfo.OnlineBeatmapID = beatmapId;
            }
        }

        /// <summary>
        /// Retrieve a performance calculator
        /// </summary>
        /// <param name="score">Score to calculate performance for.</param>
        /// <param name="rulesetOverwrite">Use a custom ruleset to calculate performance. Useful for converts.</param>
        public PerformanceCalculator GetPerformanceCalculator(ScoreInfo score, Ruleset? rulesetOverwrite = null)
        {
            var difficultyAttributes = CalculateDifficulty(score.Mods);
            var ruleset = rulesetOverwrite ?? GetDefaultRuleset();
            var rulesetId = ruleset.RulesetInfo.ID;
            return rulesetId switch
            {
                0 => new OsuPerformanceCalculator(ruleset, difficultyAttributes, score),
                1 => new TaikoPerformanceCalculator(ruleset, difficultyAttributes, score),
                2 => new CatchPerformanceCalculator(ruleset, difficultyAttributes, score),
                3 => new ManiaPerformanceCalculator(ruleset, difficultyAttributes, score),
                _ => throw new ArgumentOutOfRangeException($"{nameof(rulesetId)} must be a supported ruleset ID, {rulesetId} is not one!")
            };
        }

        public DifficultyAttributes CalculateDifficulty(params Mod[] mods) => difficultyCalculator.Calculate(mods);

        public string GetOnlineUrl(bool forceFullUrl = false)
        {
            try
            {
                return $"https://osu.ppy.sh/beatmapsets/{BeatmapInfo.BeatmapSet.OnlineBeatmapSetID}"
                       + $"#{BuiltInRulesets[BeatmapInfo.RulesetID].ShortName}/{beatmap.BeatmapInfo.OnlineBeatmapID}";
            }
            catch
            {
                if (forceFullUrl)
                {
                    throw;
                }

                return $"https://osu.ppy.sh/b/{beatmap.BeatmapInfo.OnlineBeatmapID}";
            }
        }

        public Ruleset GetDefaultRuleset() => BuiltInRulesets[beatmap.BeatmapInfo.RulesetID];
        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null!;
        protected override Track GetBeatmapTrack() => null!;
        protected override ISkin GetSkin() => null!;
        public override Stream GetStream(string storagePath) => null!;
    }
}