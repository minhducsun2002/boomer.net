using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;

namespace Pepper.Commons.Osu
{
    public class SharedConstants
    {
        internal static readonly Ruleset[] BuiltInRulesets =
        {
            new OsuRuleset(),
            new TaikoRuleset(),
            new CatchRuleset(),
            new ManiaRuleset()
        };

        internal static readonly Dictionary<int, ImmutableArray<Mod>> BuiltInMods = BuiltInRulesets
            .ToDictionary(
                ruleset => ruleset.RulesetInfo.OnlineID,
                ruleset => ruleset.CreateAllMods().ToImmutableArray()
            );

        public static readonly Version? OsuVersion;

        static SharedConstants()
        {
            var version = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.FullName?.StartsWith(nameof(osu)) == true)
                ?.GetName()
                .Version;
            if (version is not null)
            {
                OsuVersion = new Version(version.Major, version.Minor, version.Build);
            }
        }


    }
}