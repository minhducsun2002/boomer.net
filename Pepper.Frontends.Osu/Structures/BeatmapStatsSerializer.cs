using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Catch.Difficulty;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Difficulty;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Difficulty;
using Pepper.Commons.Extensions;
using Pepper.Frontends.Osu.Structures.TypeParsers;

namespace Pepper.Frontends.Osu.Structures
{
    public class BeatmapStatsSerializer
    {
        public IEnumerable<Mod>? Mods { get; init; }
        public DifficultyAttributes? DifficultyOverwrite { get; init; }
        public ControlPointInfo? ControlPointInfo { get; init; }

        private readonly IBeatmapInfo beatmapInfo;
        private readonly BeatmapDifficulty difficulty;

        public BeatmapStatsSerializer(IBeatmapInfo beatmapInfo)
        {
            this.beatmapInfo = beatmapInfo;
            difficulty = new BeatmapDifficulty(beatmapInfo.Difficulty);
        }

        private double GetMapSpeed()
        {
            var speedChange = 1.0;
            foreach (var mod in Mods ?? Array.Empty<Mod>())
            {
                if (mod is ModRateAdjust rateAdjustment)
                {
                    speedChange *= rateAdjustment.SpeedChange.Value;
                }
            }

            return speedChange;
        }

        public string Serialize(
            bool formatted,
            string delimiter = " • ",
            StatFilter serializationOptions = StatFilter.Length | StatFilter.Statistics | StatFilter.BPM | StatFilter.StarRating)
        {
            var builder = new StringBuilder();
            var speedChange = GetMapSpeed();

            if ((serializationOptions & StatFilter.StarRating) == StatFilter.StarRating)
            {
                var starRating = DifficultyOverwrite?.StarRating ?? beatmapInfo.StarRating;
                builder.Append($"{starRating:F2}⭐ ");
            }

            if ((serializationOptions & StatFilter.Combo) == StatFilter.Combo)
            {
                var maxCombo = DifficultyOverwrite?.MaxCombo ?? (beatmapInfo as APIBeatmap)?.MaxCombo;
                if (maxCombo is not null)
                {
                    if (builder.Length != 0)
                    {
                        builder.Append(delimiter);
                    }
                    builder.Append(formatted ? $"**{maxCombo}**x" : $"{maxCombo}x");
                }
            }

            if ((serializationOptions & StatFilter.Statistics) == StatFilter.Statistics)
            {
                if (builder.Length != 0)
                {
                    builder.Append(delimiter);
                }

                foreach (var mod in Mods ?? Array.Empty<Mod>())
                {
                    if (mod is IApplicableToDifficulty m)
                    {
                        m.ApplyToDifficulty(difficulty);
                    }
                }

                switch (DifficultyOverwrite)
                {
                    case OsuDifficultyAttributes osuDifficulty:
                        difficulty.ApproachRate = (float) Math.Round(osuDifficulty.ApproachRate, 2);
                        difficulty.OverallDifficulty = (float) Math.Round(osuDifficulty.OverallDifficulty, 2);
                        break;
                    case TaikoDifficultyAttributes taikoDifficulty:
                        // difficulty.ApproachRate = (float) Math.Round(taikoDifficulty, 2);
                        break;
                    case CatchDifficultyAttributes catchDifficulty:
                        difficulty.ApproachRate = (float) Math.Round(catchDifficulty.ApproachRate, 2);
                        break;
                }

                var final = new List<(string stat, float value)>();
                var ruleset = RulesetTypeParser.SupportedRulesets[beatmapInfo.Ruleset.OnlineID];

                switch (ruleset)
                {
                    case TaikoRuleset:
                        {
                            final.Add(("OD", difficulty.OverallDifficulty));
                            final.Add(("HP", difficulty.DrainRate));
                            break;
                        }

                    default:
                        {
                            final.Add(("CS", difficulty.CircleSize));
                            final.Add(("AR", difficulty.ApproachRate));
                            final.Add(("OD", difficulty.OverallDifficulty));
                            final.Add(("HP", difficulty.DrainRate));
                            break;
                        }
                }

                var difficultyStringBuilder = new StringBuilder();
                difficultyStringBuilder.AppendJoin(
                    " ",
                    final.Select(pair => formatted ? $"`{pair.stat}`**{pair.value:0.##}**" : $"{pair.stat}{pair.value:0.##}")
                );

                builder.Append(difficultyStringBuilder.ToString());
            }

            if ((serializationOptions & StatFilter.BPM) == StatFilter.BPM)
            {
                if (builder.Length != 0)
                {
                    builder.Append(delimiter);
                }

                var bpm = ControlPointInfo?.BPMMinimum ?? beatmapInfo.BPM;
                var bpmText = formatted ? $"**{bpm * speedChange:0.##}**" : $"{bpm * speedChange:0.##}";
                if (ControlPointInfo != null)
                {
                    double min = ControlPointInfo.BPMMinimum, max = ControlPointInfo.BPMMaximum;
                    if (max - min >= 2.0)
                    {
                        bpmText = formatted
                            ? $"**{min * speedChange:0.##}**-**{max * speedChange:0.##}**"
                            : $"{min * speedChange:0.##}-{max * speedChange:0.##}";

                    }
                }
                builder.Append($"{bpmText} BPM");
            }


            if ((serializationOptions & StatFilter.Length) == StatFilter.Length)
            {
                if (builder.Length != 0)
                {
                    builder.Append(delimiter);
                }

                var length = (beatmapInfo.Length / speedChange);
                builder.Append(
                        $@":clock3: {length.SerializeAsMiliseconds()}"
                );
            }

            return builder.ToString();
        }
    }
}