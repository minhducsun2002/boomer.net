using System;
using System.IO;
using System.Linq;
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
                beatmap.BeatmapInfo.OnlineID = beatmapId.Value;
            }
        }

        /// <summary>
        /// Calculate performance points for a score.
        /// </summary>
        /// <param name="score">Score to calculate performance for.</param>
        /// <param name="rulesetOverwrite">Use a custom ruleset to calculate performance. Useful for converts.</param>
        /// <param name="isClassic">Whether to calculate with classic mods.</param>
        public double CalculatePerformance(ScoreInfo score, Ruleset? rulesetOverwrite = null, bool isClassic = true)
        {
            var ruleset = rulesetOverwrite ?? GetDefaultRuleset();
            var rulesetId = ruleset.RulesetInfo.OnlineID;
            var difficultyAttributes = CalculateDifficulty(rulesetId, isClassic, score.Mods);
            PerformanceCalculator calculator = rulesetId switch
            {
                0 => new OsuPerformanceCalculator(),
                1 => new TaikoPerformanceCalculator(),
                2 => new CatchPerformanceCalculator(),
                3 => new ManiaPerformanceCalculator(),
                _ => throw new ArgumentOutOfRangeException($"{nameof(rulesetId)} must be a supported ruleset ID, {rulesetId} is not one!")
            };

            var calc = calculator.Calculate(score, difficultyAttributes);
            return calc.Total;
        }

        public DifficultyAttributes CalculateDifficulty(int rulesetId, bool isClassic = true, params Mod[] mods)
        {
            var modToCalc = mods.AsEnumerable();
            if (isClassic && !mods.Any(m => m is ModClassic))
            {
                modToCalc = modToCalc.Append(APIClient.BuiltInMods[rulesetId].OfType<ModClassic>().First());
            }

            return BuiltInRulesets[rulesetId]
                .CreateDifficultyCalculator(this)
                .Calculate(modToCalc);
        }

        public string GetOnlineUrl(bool forceFullUrl = false, Ruleset? rulesetOverwrite = null)
        {
            var ruleset = rulesetOverwrite ?? BuiltInRulesets[BeatmapInfo.Ruleset.OnlineID];
            try
            {
                return $"https://osu.ppy.sh/beatmapsets/{BeatmapInfo.BeatmapSet?.OnlineID}"
                       + $"#{ruleset.ShortName}/{beatmap.BeatmapInfo?.OnlineID}";
            }
            catch
            {
                if (forceFullUrl)
                {
                    throw;
                }

                return $"https://osu.ppy.sh/b/{beatmap.BeatmapInfo?.OnlineID}";
            }
        }

        public Ruleset GetDefaultRuleset() => BuiltInRulesets[beatmap.BeatmapInfo.Ruleset.OnlineID];
        protected override IBeatmap GetBeatmap() => beatmap;
        public override Texture GetBackground() => null!;
        protected override Track GetBeatmapTrack() => null!;
        protected override ISkin GetSkin() => null!;
        public override Stream GetStream(string storagePath) => null!;
    }
}