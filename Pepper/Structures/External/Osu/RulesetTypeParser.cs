using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class RulesetTypeParser : DiscordTypeParser<Ruleset>
    {
        public static readonly Ruleset[] SupportedRulesets = 
        {
            new OsuRuleset(),
            new TaikoRuleset(),
            new CatchRuleset(),
            new ManiaRuleset()
        };
        
        public override ValueTask<TypeParserResult<Ruleset>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
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