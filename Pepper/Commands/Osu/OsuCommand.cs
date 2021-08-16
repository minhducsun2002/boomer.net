using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AJ.Code;
using Disqord;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
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
using Pepper.Services.Osu.API;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;

namespace Pepper.Commands.Osu
{
    [PrefixCategory("osu")]
    [Category("osu!")]
    public abstract class OsuCommand : Command
    {
        public OsuCommand(APIService service) => APIService = service;
        protected readonly APIService APIService;
        protected static Ruleset[] Rulesets => RulesetTypeParser.SupportedRulesets;

        protected static string ResolveEarthEmoji(string countryCode)
        {
            var continent = Country.GetCountryInfoForAlpha2Code(countryCode)?.ContinentCode;
            return continent switch
            {
                ContinentCode.AF => ":earth_africa:",
                ContinentCode.EU => ":earth_africa:",
                ContinentCode.NA => ":earth_americas:",
                ContinentCode.SA => ":earth_americas:",
                _ => ":earth_asia:"
            };
        }

        public static string SerializeBeatmapStats(
            BeatmapInfo map, DifficultyAttributes? difficultyOverwrite = null,
            ControlPointInfo? controlPointInfo = null,
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

            var speedChange = 1.0;
            if (difficultyOverwrite != null)
                foreach (var mod in difficultyOverwrite.Mods)
                    if (mod is ModRateAdjust rateAdjustment)
                        speedChange *= rateAdjustment.SpeedChange.Value;
            var mapLength = map.Length / speedChange;

            var bpm = $"**{map.BPM * speedChange:0.##}**";
            if (controlPointInfo != null)
            {
                double min = controlPointInfo.BPMMinimum, max = controlPointInfo.BPMMaximum;
                bpm = max - min < 2.0 ? $"**{min * speedChange:0.##}**" : $"**{min * speedChange:0.##}**-**{max * speedChange:0.##}**";
            }
            
            return
                $"{difficultyOverwrite?.StarRating ?? map.StarDifficulty:F2} :star: "
                + $" {delimiter} `CS`**{diff.CircleSize}** `AR`**{diff.ApproachRate}** `OD`**{diff.OverallDifficulty}** `HP`**{diff.DrainRate}**"
                + $" {delimiter} {bpm} BPM"
                + (showLength 
                    ? $@" {delimiter} :clock3: {
                        Math.Floor(mapLength / 60000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                    }:{((long) mapLength % 60000 / 1000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}" 
                    : "");
        }

        public static string SerializeBeatmapStats(APIBeatmapSet set, APIBeatmap diff, bool showLength = true, bool showBPM = true, char delimiter = '•')
            => $"{diff.StarDifficulty:F2}⭐ "
               + $" {delimiter} CS{diff.CircleSize} AR{diff.ApproachRate} OD{diff.OverallDifficulty} HP{diff.DrainRate}"
               + (showBPM ? $" {delimiter} {set.Bpm} BPM" : "")
               + (showLength 
                   ? $@" {delimiter} :clock3: {
                       (diff.TotalLengthInSeconds / 60).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                   }:{(diff.TotalLengthInSeconds % 60).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}" 
                   : "");

        protected static string SerializeHitStats(Dictionary<string, int> statistics)
            => $"**{statistics["count_300"]}**/**{statistics["count_100"]}**/**{statistics["count_50"]}**/**{statistics["count_miss"]}**";

        public static string SerializeTimestamp(DateTimeOffset timestamp, bool utcHint = true)
            => timestamp.ToUniversalTime().ToString($"HH:mm:ss, dd/MM/yyyy{(utcHint ? " 'UTC'" : "")}", CultureInfo.InvariantCulture);

        public static LocalEmbedAuthor SerializeAuthorBuilder(osu.Game.Users.User user)
            => new()
            {
                IconUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? $"https://a.ppy.sh/{user.Id}" : user.AvatarUrl,
                Name = user.Username,
                Url = $"https://osu.ppy.sh/users/{user.Id}"
            };

        public static PerformanceCalculator GetPerformanceCalculator(int rulesetId, DifficultyAttributes beatmapAttributes, ScoreInfo score)
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