using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AJ.Code;
using Disqord;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mania.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring;
using Pepper.Services.Osu;
using Pepper.Structures;
using Pepper.Structures.Commands;

namespace Pepper.Commands.Osu
{
    [PrefixCategory("osu")]
    [Category("osu!")]
    public abstract class OsuCommand : Command
    {
        public ApiService ApiService { get; set; }
        protected static readonly Ruleset[] Rulesets = { new OsuRuleset(), new TaikoRuleset(), new CatchRuleset(), new ManiaRuleset() };
        
        
        protected static string ResolveEarthEmoji(ContinentCode continent)
            => continent switch
            {
                ContinentCode.AF => ":earth_africa:",
                ContinentCode.EU => ":earth_africa:",
                ContinentCode.NA => ":earth_americas:",
                ContinentCode.SA => ":earth_americas:",
                _ => ":earth_asia:"
            };

        protected static string SerializeBeatmapStats(
            BeatmapInfo map, DifficultyAttributes? difficultyOverwrite = null,
            bool showLength = true, char delimiter = '•')
        {
            var diff = map.BaseDifficulty;
            switch (difficultyOverwrite)
            {
                case OsuDifficultyAttributes osuDifficulty:
                    diff.ApproachRate = (float) Math.Round(osuDifficulty.ApproachRate, 2);
                    diff.OverallDifficulty = (float) Math.Round(osuDifficulty.OverallDifficulty, 2);
                    break;
                case TaikoDifficultyAttributes taikoDifficulty:
                    diff.ApproachRate = (float) Math.Round(taikoDifficulty.ApproachRate, 2);
                    break;
                case CatchDifficultyAttributes catchDifficulty:
                    diff.ApproachRate = (float) Math.Round(catchDifficulty.ApproachRate, 2);
                    break;
            }
            
            return
                $"{map.StarDifficulty:F2} :star: "
                + $" {delimiter} `AR`**{diff.ApproachRate}** `CS`**{diff.CircleSize}** `OD`**{diff.OverallDifficulty}** `HP`**{diff.DrainRate}**"
                + $" {delimiter} **{map.BPM}** BPM"
                + (showLength 
                    ? $@" {delimiter} :clock3: {
                        Math.Floor(map.Length / 60000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                    }:{((long) map.Length % 60000 / 1000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}" 
                    : "");
        }

        protected static string SerializeHitStats(Dictionary<string, int> statistics)
            => $"**{statistics["count_300"]}**/**{statistics["count_100"]}**/**{statistics["count_50"]}**/**{statistics["count_miss"]}**";

        protected static string SerializeTimestamp(DateTimeOffset timestamp, bool UTCHint = true)
            => timestamp.ToUniversalTime().ToString($"HH:mm:ss, dd/MM/yyyy{(UTCHint ? " 'UTC'" : "")}", CultureInfo.InvariantCulture);

        protected static LocalEmbedAuthor SerializeAuthorBuilder(osu.Game.Users.User user)
            => new()
            {
                IconUrl = user.AvatarUrl,
                Name = user.Username,
                Url = $"https://osu.ppy.sh/users/{user.Id}"
            };

        protected static PerformanceCalculator GetPerformanceCalculator(int rulesetId, DifficultyAttributes beatmapAttributes, ScoreInfo score)
        {
            var ruleset = Rulesets[rulesetId];
            return rulesetId switch
            {
                0 => new OsuPerformanceCalculator(ruleset, beatmapAttributes, score),
                1 => new TaikoPerformanceCalculator(ruleset, beatmapAttributes, score),
                2 => new CatchPerformanceCalculator(ruleset, beatmapAttributes, score),
                3 => new ManiaPerformanceCalculator(ruleset, beatmapAttributes, score),
                _ => throw new ArgumentException($"{nameof(rulesetId)} must be a supported ruleset ID, {rulesetId} is not one!")
            };
        }

        protected static Mod[] ResolveMods(Ruleset ruleset, params string[] modStrings)
        {
            var allMods = ruleset.GetAllMods().ToArray();
            return modStrings
                .Select(modString => 
                    allMods.FirstOrDefault(mod => string.Equals(mod.Acronym, modString, StringComparison.InvariantCultureIgnoreCase)))
                .Where(mod => mod != null)
                .ToArray()!;
        }
    }
}