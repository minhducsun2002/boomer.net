using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;

namespace Pepper.Frontends.Osu.Structures
{
    public class UnknownRuleset : Ruleset
    {
        public override IEnumerable<Mod> GetModsFor(ModType type) => Array.Empty<Mod>();
        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => throw new NotImplementedException();
        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => throw new NotImplementedException();
        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => throw new NotImplementedException();
        public override string Description => "";
        public override string ShortName => "Unknown ruleset";
    }
}