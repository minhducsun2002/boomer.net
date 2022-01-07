using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

namespace Pepper.Commons.Osu
{
    /// <summary>
    /// A service to parse mod string sequences ("DT", "HR") into cached mods. 
    /// </summary>
    public class ModParserService
    {
        private readonly Dictionary<int, Dictionary<string, Mod>> modCache = new();

        public ModParserService()
        {
            foreach (var ruleset in APIClient.BuiltInRulesets)
            {
                var key = ruleset.RulesetInfo.OnlineID;
                modCache[key] = new Dictionary<string, Mod>();
                foreach (var mod in ruleset.CreateAllMods())
                {
                    modCache[key][mod.Acronym.ToLowerInvariant()] = mod;
                }
            }
        }

        public Mod[] ResolveMods(Ruleset ruleset, IEnumerable<string> modStrings)
        {
            return modStrings
                .Select(modString =>
                {
                    var s = modString.ToLowerInvariant();
                    return modCache[ruleset.RulesetInfo.OnlineID].TryGetValue(s, out var mod) ? mod : null;
                })
                .Where(mod => mod != null)
                .ToArray()!;
        }
    }
}