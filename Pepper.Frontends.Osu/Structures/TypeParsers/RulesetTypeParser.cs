using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using Qmmands;
using Qmmands.Default;

namespace Pepper.Frontends.Osu.Structures.TypeParsers
{
    // TODO: Write tests for this
    public class RulesetTypeParser : TypeParser<Ruleset>
    {
        public static readonly Ruleset[] SupportedRulesets =
        {
            new OsuRuleset(),
            new TaikoRuleset(),
            new CatchRuleset(),
            new ManiaRuleset()
        };

        private static readonly (string, Ruleset)[] RulesetNames =
        {
            ("osu", SupportedRulesets[0]),
            ("std", SupportedRulesets[0]),
            ("taiko", SupportedRulesets[1]),
            ("fruits", SupportedRulesets[2]),
            ("catch", SupportedRulesets[2]),
            ("ctb", SupportedRulesets[2]),
            ("mania", SupportedRulesets[3])
        };

        public override ValueTask<ITypeParserResult<Ruleset>> ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            Ruleset defaultRuleset = new OsuRuleset();
            try
            {
                defaultRuleset = RulesetNames
                    .First(pair => pair.Item1.Equals(input.ToString(), StringComparison.OrdinalIgnoreCase))
                    .Item2;

            }
            catch { /* ignored, defaults to osu! */ }

            return Success(defaultRuleset);
        }
    }
}