using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Pepper.Services.Osu;
using Pepper.Services.Osu.API;
using Pepper.Structures.External.Osu.Extensions;
using WorkingBeatmap = Pepper.Structures.External.Osu.WorkingBeatmap;

namespace Pepper.Commands.Osu
{
    public abstract class OsuScoreCommand : OsuCommand
    {
        protected OsuScoreCommand(APIService service) : base(service) {}

        protected async Task<DiscordCommandResult> SingleScore(APILegacyScoreInfo sc)
        {
            var b = sc.Beatmap!;
            var workingBeatmap = await APIService.GetBeatmap(b.OnlineBeatmapID!.Value);
            var ruleset = Rulesets[sc.OnlineRulesetID];

            var mods = ResolveMods(ruleset, sc.Mods);
            var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods);

            b.StarDifficulty = difficulty.StarRating;

            return SingleScoreOutput(
                sc.User,
                artist: b.Metadata.Artist, title: b.Metadata.Title, version: b.Version,
                rank: $"{sc.Rank}",
                mods: mods, accuracy: sc.Accuracy, perfect: sc.Perfect, totalScore: sc.TotalScore,
                scoreMaxCombo: sc.MaxCombo, pp: sc.PP,
                timestamp: sc.Date, ruleset: ruleset,
                workingBeatmap: workingBeatmap, statistics: sc.Statistics, scoreId: sc.OnlineBestScoreID
            );
        }

        protected async Task<DiscordCommandResult> SingleScore(osu.Game.Users.User user, OsuSharp.Score sc)
        {
            var workingBeatmap = await APIService.GetBeatmap(sc.BeatmapId);
            var b = workingBeatmap.Beatmap.BeatmapInfo;
            var ruleset = Rulesets[(int) sc.GameMode];
            var mods = ruleset.ConvertFromLegacyMods((LegacyMods) sc.Mods)!.ToArray();
            var statistics = new Dictionary<string, int>
            {
                {"count_300", sc.Count300},
                {"count_100", sc.Count100},
                {"count_50", sc.Count50},
                {"count_geki", sc.Geki},
                {"count_katu", sc.Katu},
                {"count_miss", sc.Miss},
            };
            return SingleScoreOutput(
                user,
                artist: b.Metadata.Artist, title: b.Metadata.Title, version: b.Version,
                rank: $"{sc.Rank}",
                mods: mods, accuracy: sc.Accuracy / 100, perfect: sc.Perfect, totalScore: sc.TotalScore,
                scoreMaxCombo: sc.MaxCombo ?? 0, pp: sc.PerformancePoints,
                timestamp: sc.Date!.Value, ruleset: Rulesets[(int) sc.GameMode],
                workingBeatmap: workingBeatmap, statistics: statistics, scoreId: null
            );
        }

        private DiscordCommandResult SingleScoreOutput(
            osu.Game.Users.User user,
            string rank, string artist, string title, string version, Mod[] mods,
            WorkingBeatmap workingBeatmap, DateTimeOffset timestamp,
            Ruleset ruleset,
            int scoreMaxCombo, bool perfect, double accuracy, long totalScore,
            Dictionary<string, int> statistics,
            long? scoreId = null, double? pp = null
        )
        {
            var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods);
            double? fullComboPP = pp;
            var calculated = false;
            var rulesetId = ruleset.RulesetInfo.ID!.Value;

            if (!pp.HasValue || !perfect)
            {
                if (!pp.HasValue)
                {
                    var score = new ScoreInfo {Mods = mods, MaxCombo = scoreMaxCombo, Accuracy = accuracy};
                    score.SetStatistics(statistics);
                    var performanceCalculator = GetPerformanceCalculator(rulesetId, difficulty, score);
                    pp = performanceCalculator.Calculate();
                    calculated = true;
                }

                if (!perfect)
                {
                    var fcScore = new ScoreInfo {Mods = mods, MaxCombo = scoreMaxCombo, Accuracy = accuracy};
                    fcScore.SetStatistics(statistics);
                    fcScore.SetCount300((int) (fcScore.GetCount300() + fcScore.GetCountMiss())!);
                    fcScore.SetCountMiss(0);
                    var performanceCalculator = GetPerformanceCalculator(rulesetId, difficulty, fcScore);
                    fullComboPP = performanceCalculator.Calculate();
                }
            }

            return Reply(new LocalEmbed
            {
                Author = SerializeAuthorBuilder(user),
                Title = $"[**{rank}**] {artist} - {title} [{version}]"
                        + (mods.Length != 0 ? "+" + string.Join("", mods.Select(mod => mod.Acronym)) : ""),
                Url = workingBeatmap.GetOnlineUrl(),
                ThumbnailUrl =
                    $"https://assets.ppy.sh/beatmaps/{workingBeatmap.BeatmapSetInfo?.OnlineBeatmapSetID}/covers/list@2x.jpg?",
                Timestamp = timestamp,
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Statistics",
                        Value =
                            $"**{scoreMaxCombo}**x/**{difficulty.MaxCombo}**x • [{SerializeHitStats(statistics)}] • **{accuracy * 100:F3}**%"
                            + $"\n**{pp:F2}**pp {(calculated ? " (?)" : "")}"
                            + (perfect ? "" : $" / **{fullComboPP:F3}**pp (?)")
                            + $" • **`{totalScore:n0}`**"
                            + (scoreId == null
                                ? ""
                                : $" • [[**Score**]](https://osu.ppy.sh/scores/{ruleset.ShortName}/{scoreId})")
                    },
                    new()
                    {
                        Name = "Beatmap information",
                        Value = SerializeBeatmapStats(workingBeatmap.BeatmapInfo, difficulty, workingBeatmap.Beatmap.ControlPointInfo)
                    }
                }
            });
        }
    }
}