using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class RulesetTypeParser : TypeParser<Ruleset>
    {
        public static readonly RulesetTypeParser Instance = new RulesetTypeParser();

        public static readonly Ruleset[] SupportedRulesets = 
        {
            new OsuRuleset(),
            new TaikoRuleset(),
            new CatchRuleset(),
            new ManiaRuleset()
        };
        
        public override ValueTask<TypeParserResult<Ruleset>> ParseAsync(Parameter parameter, string value, Qmmands.CommandContext context)
        {
            Ruleset defaultRuleset = new OsuRuleset();
            try
            {
                defaultRuleset = SupportedRulesets
                    .First(ruleset =>
                        string.Equals(ruleset.ShortName, value, StringComparison.InvariantCultureIgnoreCase));
                
            }
            catch { /* ignored, defaults to osu! */ }

            return TypeParserResult<Ruleset>.Successful(defaultRuleset);
        }
    }
}