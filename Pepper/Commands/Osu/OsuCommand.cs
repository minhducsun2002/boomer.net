using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AJ.Code;
using Disqord;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Difficulty;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Metadata;
using Pepper.Structures.External.Osu;
using Pepper.Structures.External.Osu.Extensions;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;

namespace Pepper.Commands.Osu
{
    [PrefixCategory("osu")]
    [Category("osu!")]
    public abstract class OsuCommand : Command
    {
        public OsuCommand(APIClientStore apiClientStore) => APIClientStore = apiClientStore;
        protected readonly APIClientStore APIClientStore;
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
            IBeatmapInfo map,
            IEnumerable<Mod>? mods = null,
            DifficultyAttributes? difficultyOverwrite = null,
            ControlPointInfo? controlPointInfo = null,
            bool showLength = true, char delimiter = '•')
        {
            var diff = new BeatmapDifficulty(map.Difficulty);

            var speedChange = 1.0;
            foreach (var mod in mods ?? Array.Empty<Mod>())
            {
                if (mod is IApplicableToDifficulty m)
                {
                    m.ApplyToDifficulty(diff);
                }

                if (mod is ModRateAdjust rateAdjustment)
                {
                    speedChange *= rateAdjustment.SpeedChange.Value;
                }
            }

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

            var mapLength = map.Length / speedChange;

            var bpm = $"**{map.BPM * speedChange:0.##}**";
            if (controlPointInfo != null)
            {
                double min = controlPointInfo.BPMMinimum, max = controlPointInfo.BPMMaximum;
                bpm = max - min < 2.0 ? $"**{min * speedChange:0.##}**" : $"**{min * speedChange:0.##}**-**{max * speedChange:0.##}**";
            }

            return
                $"{difficultyOverwrite?.StarRating ?? map.StarRating:F2} :star: "
                + $" {delimiter} `CS`**{diff.CircleSize:0.##}** `AR`**{diff.ApproachRate:0.##}** `OD`**{diff.OverallDifficulty:0.##}** `HP`**{diff.DrainRate:0.##}**"
                + $" {delimiter} {bpm} BPM"
                + (showLength
                    ? $@" {delimiter} :clock3: {
                        Math.Floor(mapLength / 60000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                    }:{((long) mapLength % 60000 / 1000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}"
                    : "");
        }

        public static string SerializeBeatmapStats(
            APIBeatmap diff,
            bool formatted,
            APIBeatmapSet? beatmapset = null,
            string delimiter = " • ",
            BeatmapSerializationOptions serializationOptions = BeatmapSerializationOptions.Length |
                                                               BeatmapSerializationOptions.Statistics |
                                                               BeatmapSerializationOptions.BPM |
                                                               BeatmapSerializationOptions.StarRating
        )
        {
            var builder = new StringBuilder();
            if ((serializationOptions & BeatmapSerializationOptions.StarRating) == BeatmapSerializationOptions.StarRating)
            {
                builder.Append($"{diff.StarRating:F2}⭐ ");
            }

            if ((serializationOptions & BeatmapSerializationOptions.Combo) == BeatmapSerializationOptions.Combo)
            {
                if (diff.MaxCombo is not null)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(delimiter);
                    }
                    builder.Append(formatted ? $"**{diff.MaxCombo}**x" : $"{diff.MaxCombo}x");
                }
            }

            if ((serializationOptions & BeatmapSerializationOptions.Statistics) == BeatmapSerializationOptions.Statistics)
            {
                if (builder.Length != 0)
                {
                    builder.Append(delimiter);
                }
                builder.Append(
                    formatted
                    ? $"`CS`**{diff.CircleSize:0.##}** `AR`**{diff.ApproachRate:0.##}** `OD`**{diff.OverallDifficulty:0.##}** `HP`**{diff.DrainRate:0.##}**"
                    : $"CS{diff.CircleSize:0.##} AR{diff.ApproachRate:0.##} OD{diff.OverallDifficulty:0.##} HP{diff.DrainRate:0.##}");
            }

            if ((serializationOptions & BeatmapSerializationOptions.BPM) == BeatmapSerializationOptions.BPM)
            {
                if (builder.Length != 0)
                {
                    builder.Append(delimiter);
                }

                var bpm = Math.Max(diff.BPM, beatmapset?.BPM ?? 0);
                builder.Append(formatted ? $"**{bpm}** BPM" : $"{bpm} BPM");
            }


            if ((serializationOptions & BeatmapSerializationOptions.Length) == BeatmapSerializationOptions.Length)
            {
                if (builder.Length != 0)
                {
                    builder.Append(delimiter);
                }
                builder.Append(
                        $@":clock3: {
                            ((long) diff.Length / 60000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                        }:{((long) diff.Length % 60000 / 1000).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}"
                );
            }

            return builder.ToString();
        }

        protected static string SerializeHitStats(Dictionary<string, int> statistics, RulesetInfo rulesetInfo)
        {
            var sc = new ScoreInfo
            {
                Ruleset = rulesetInfo
            }.WithStatistics(statistics);
            var displayStats = sc.GetStatisticsForDisplay()
                .Select(s => s.Result)
                .Where(r => !r.IsBonus())
                .Select(hitResult => $"**{(sc.Statistics.TryGetValue(hitResult, out var value) ? value : 0)}**");
            return string.Join('/', displayStats);
        }

        public static string SerializeTimestamp(DateTimeOffset? timestamp, bool utcHint = true)
            => (timestamp ?? DateTimeOffset.UnixEpoch)
                .ToUniversalTime()
                .ToString($"HH:mm:ss, dd/MM/yyyy{(utcHint ? " 'UTC'" : "")}", CultureInfo.InvariantCulture);

        public static LocalEmbedAuthor SerializeAuthorBuilder(APIUser user)
        {
            var avatarUrl = user.AvatarUrl;
            var userUrl = $"https://osu.ppy.sh/users/{user.Id}";
            if (user is Commons.Osu.API.APIUser overriddenUserInstance)
            {
                avatarUrl = overriddenUserInstance.AvatarUrl;
                userUrl = overriddenUserInstance.PublicUrl;
            }
            var embedAuthor = new LocalEmbedAuthor
            {
                IconUrl = avatarUrl,
                Name = $"{user.Username}" + ((user.Statistics.PP ?? decimal.Zero) == decimal.Zero ? "" : $" ({user.Statistics.PP}pp)"),
                Url = userUrl
            };
            return embedAuthor;
        }
    }
}