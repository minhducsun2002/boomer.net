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
            Ruleset defaultRuleset = new UnknownRuleset();
            try
            {
                var res = ResolveRuleset(input);
                if (res != null)
                {
                    defaultRuleset = res;
                }

            }
            catch { /* Leave it for further processing down the line */ }

            return Success(defaultRuleset);
        }

        public static Ruleset? ResolveRuleset(ReadOnlyMemory<char> slug)
        {
            var result = RulesetNames
                .FirstOrDefault(pair => pair.Item1.Equals(slug.ToString(), StringComparison.OrdinalIgnoreCase))
                .Item2;

            return result;
        }
    }
}