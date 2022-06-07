using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets;

namespace Pepper.Commons.Osu
{
    internal class RulesetStore : osu.Game.Rulesets.RulesetStore
    {
        public override IEnumerable<RulesetInfo> AvailableRulesets { get; } = APIClient.BuiltInRulesets.Select(r => r.RulesetInfo);
    }
}