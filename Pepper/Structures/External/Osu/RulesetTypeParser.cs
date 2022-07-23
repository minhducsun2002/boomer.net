using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using Qmmands;
using Qmmands.Default;

namespace Pepper.Structures.External.Osu
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

        public override ValueTask<ITypeParserResult<Ruleset>> ParseAsync(ICommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            Ruleset defaultRuleset = new OsuRuleset();
            try
            {
                defaultRuleset = SupportedRulesets
                    .First(ruleset => string.Equals(ruleset.ShortName, new string(input.Span), StringComparison.InvariantCultureIgnoreCase));

            }
            catch { /* ignored, defaults to osu! */ }

            return Success(defaultRuleset);
        }
    }
}