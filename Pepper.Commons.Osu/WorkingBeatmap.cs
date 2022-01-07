using System;
using System.IO;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring;
using osu.Game.Skinning;

namespace Pepper.Commons.Osu
{
    public partial class WorkingBeatmap : osu.Game.Beatmaps.WorkingBeatmap
    {
        private readonly Beatmap beatmap;
        private static Ruleset[] BuiltInRulesets => APIClient.BuiltInRulesets;

        private WorkingBeatmap(Beatmap beatmap, int? beatmapId = null) : base(beatmap.BeatmapInfo, null)
        {
            this.beatmap = beatmap;
            this.beatmap.BeatmapInfo.Ruleset = GetDefaultRuleset().RulesetInfo;

            if (beatmapId.HasValue)
            {
                beatmap.BeatmapInfo.OnlineBeatmapID = beatmapId;
            }
        }

        /// <summary>
        /// Calculate performance points for a score.
        /// </summary>
        /// <param name="score">Score to calculate performance for.</param>
        /// <param name="rulesetOverwrite">Use a custom ruleset to calculate performance. Useful for converts.</param>
        public double CalculatePerformance(ScoreInfo score, Ruleset? rulesetOverwrite = null)
        {
            var ruleset = rulesetOverwrite ?? GetDefaultRuleset();
            var rulesetId = ruleset.RulesetInfo.ID;
            var difficultyAttributes = CalculateDifficulty(rulesetId!.Value, score.Mods);
            PerformanceCalculator calculator = rulesetId switch
            {
                0 => new OsuPerformanceCalculator(ruleset, difficultyAttributes, score),
                1 => new TaikoPerformanceCalculator(ruleset, difficultyAttributes, score),
                2 => new CatchPerformanceCalculator(ruleset, difficultyAttributes, score),
                3 => new ManiaPerformanceCalculator(ruleset, difficultyAttributes, score),
                _ => throw new ArgumentOutOfRangeException($"{nameof(rulesetId)} must be a supported ruleset ID, {rulesetId} is not one!")
            };

            return calculator.Calculate();
        }

        public DifficultyAttributes CalculateDifficulty(int rulesetId, params Mod[] mods) => BuiltInRulesets[rulesetId]
            .CreateDifficultyCalculator(this)
            .Calculate(mods);

        public string GetOnlineUrl(bool forceFullUrl = false, Ruleset? rulesetOverwrite = null)
        {
            var ruleset = rulesetOverwrite ?? BuiltInRulesets[BeatmapInfo.RulesetID];
            try
            {
                return $"https://osu.ppy.sh/beatmapsets/{BeatmapInfo.BeatmapSet.OnlineBeatmapSetID}"
                       + $"#{ruleset.ShortName}/{beatmap.BeatmapInfo.OnlineBeatmapID}";
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