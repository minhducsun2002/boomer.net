using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Pepper.Commons.Osu;
using Pepper.Services.Osu;
using Pepper.Structures.External.Osu.Extensions;
using APIScoreInfo = Pepper.Commons.Osu.API.APIScoreInfo;

namespace Pepper.Commands.Osu
{
    public abstract class OsuScoreCommand : BeatmapContextCommand
    {
        protected OsuScoreCommand(APIClientStore s, BeatmapContextProviderService b) : base(s, b) { }

        protected async Task<DiscordCommandResult> SingleScore(APIScoreInfo sc)
        {
            var b = sc.Beatmap!;
            var workingBeatmap = await APIClientStore.GetClient(GameServer.Osu).GetBeatmap(b.OnlineID);
            var ruleset = Rulesets[sc.RulesetID];

            var mods = ResolveMods(ruleset, sc.Mods.Select(mod => mod.Acronym));
            var difficulty = workingBeatmap.CalculateDifficulty(sc.RulesetID, mods);

            b.StarRating = difficulty.StarRating;
            SetBeatmapContext(b.OnlineID);
            return SingleScoreOutput(
                sc.User,
                artist: b.Metadata.Artist, title: b.Metadata.Title, version: b.DifficultyName,
                rank: $"{sc.Rank}",
                mods: mods, accuracy: sc.Accuracy, perfect: sc.Perfect, totalScore: sc.TotalScore,
                scoreMaxCombo: sc.MaxCombo, pp: sc.PP,
                timestamp: sc.Date, ruleset: ruleset,
                workingBeatmap: workingBeatmap, statistics: sc.Statistics, scoreId: sc.OnlineBestScoreID
            );
        }

        private DiscordCommandResult SingleScoreOutput(
            APIUser user,
            string rank, string artist, string title, string version, Mod[] mods,
            WorkingBeatmap workingBeatmap, DateTimeOffset timestamp,
            Ruleset ruleset,
            int scoreMaxCombo, bool perfect, double accuracy, long totalScore,
            Dictionary<string, int> statistics,
            long? scoreId = null, double? pp = null
        )
        {
            SetBeatmapContext(workingBeatmap.BeatmapInfo.OnlineBeatmapID!.Value);
            var difficulty = workingBeatmap.CalculateDifficulty(ruleset.RulesetInfo.OnlineID, mods);
            double? fullComboPP = pp;
            var calculated = false;

            if (!pp.HasValue || !perfect)
            {
                if (!pp.HasValue)
                {
                    var score = new ScoreInfo { Mods = mods, MaxCombo = scoreMaxCombo, Accuracy = accuracy };
                    score.SetStatistics(statistics);
                    var performanceCalculator = workingBeatmap.GetPerformanceCalculator(score, ruleset);
                    pp = performanceCalculator.Calculate();
                    calculated = true;
                }

                if (!perfect)
                {
                    var fcScore = new ScoreInfo { Mods = mods, MaxCombo = difficulty.MaxCombo, Accuracy = accuracy };
                    fcScore.SetStatistics(statistics);
                    fcScore.SetCount300((int) (fcScore.GetCount300() + fcScore.GetCountMiss())!);
                    fcScore.SetCountMiss(0);
                    var performanceCalculator = workingBeatmap.GetPerformanceCalculator(fcScore, ruleset);
                    fullComboPP = performanceCalculator.Calculate();
                }
            }

            int hitCounts, totalHitCounts = workingBeatmap.Beatmap.HitObjects.Count;
            {
                var temporaryScore = new ScoreInfo();
                temporaryScore.SetStatistics(statistics);
                hitCounts = (temporaryScore.GetCount50() + temporaryScore.GetCount100() +
                                  temporaryScore.GetCount300() + temporaryScore.GetCountMiss())!.Value;
            }

            return Reply(new LocalEmbed
            {
                Author = SerializeAuthorBuilder(user),
                Title = $"[**{rank}**] {artist} - {title} [{version}]"
                        + (mods.Length != 0 ? "+" + string.Join("", mods.Select(mod => mod.Acronym)) : ""),
                Url = workingBeatmap.GetOnlineUrl(),
                ThumbnailUrl =
                    $"https://b.ppy.sh/thumb/{workingBeatmap.BeatmapSetInfo.OnlineBeatmapSetID}l.jpg",
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
                },
                Footer = totalHitCounts > hitCounts
                    ? new LocalEmbedFooter().WithText($"{hitCounts / (float) totalHitCounts * 100:F2}% completed")
                    : null
            });
        }
    }
}