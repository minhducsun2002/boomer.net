using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
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
        protected readonly ModParserService ModParserService;

        protected OsuScoreCommand(APIClientStore s, BeatmapContextProviderService b, ModParserService p) : base(s, b)
        {
            ModParserService = p;
        }

        protected async Task<DiscordCommandResult> SingleScore(APIScoreInfo sc)
        {
            var b = sc.Beatmap!;
            var workingBeatmap = await APIClientStore.GetClient(GameServer.Osu).GetBeatmap(b.OnlineID);
            var ruleset = Rulesets[sc.RulesetID];

            var mods = ModParserService.ResolveMods(ruleset, sc.Mods.Select(mod => mod.Acronym));
            var difficulty = workingBeatmap.CalculateDifficulty(sc.RulesetID, mods);

            SetBeatmapContext(workingBeatmap.BeatmapInfo.OnlineBeatmapID!.Value);

            b.StarRating = difficulty.StarRating;
            double? pp = sc.PP, fullComboPP = pp;
            var calculated = false;

            if (!pp.HasValue)
            {
                pp = workingBeatmap.CalculatePerformance(
                    rulesetOverwrite: ruleset,
                    score: new ScoreInfo { Mods = mods, MaxCombo = sc.MaxCombo, Accuracy = sc.Accuracy }
                        .WithStatistics(sc.Statistics)
                );
                calculated = true;
            }

            if (!sc.Perfect)
            {
                var fcScore = new ScoreInfo { Mods = mods, MaxCombo = difficulty.MaxCombo, Accuracy = sc.Accuracy }
                    .WithStatistics(sc.Statistics);
                fcScore.SetCount300((int) (fcScore.GetCount300() + fcScore.GetCountMiss())!);
                fcScore.SetCountMiss(0);
                fullComboPP = workingBeatmap.CalculatePerformance(fcScore, ruleset);
            }

            int hitCounts, totalHitCounts = workingBeatmap.Beatmap.HitObjects.Count;
            {
                var temporaryScore = new ScoreInfo().WithStatistics(sc.Statistics);
                hitCounts = (temporaryScore.GetCount50() + temporaryScore.GetCount100() +
                             temporaryScore.GetCount300() + temporaryScore.GetCountMiss())!.Value;
            }

            return Reply(new LocalEmbed
            {
                Author = SerializeAuthorBuilder(sc.User),
                Title = $"[**{sc.Rank}**] {b.Metadata.Artist} - {b.Metadata.Title} [{b.DifficultyName}]"
                        + (mods.Length != 0 ? "+" + string.Join("", mods.Select(mod => mod.Acronym)) : ""),
                Url = workingBeatmap.GetOnlineUrl(),
                ThumbnailUrl =
                    $"https://b.ppy.sh/thumb/{workingBeatmap.BeatmapSetInfo.OnlineBeatmapSetID}l.jpg",
                Timestamp = sc.Date,
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Statistics",
                        Value =
                            $"**{sc.MaxCombo}**x/**{difficulty.MaxCombo}**x • [{SerializeHitStats(sc.Statistics)}] • **{sc.Accuracy * 100:F3}**%"
                            + $"\n**{pp:F2}**pp {(calculated ? " (?)" : "")}"
                            + (sc.Perfect ? "" : $" / **{fullComboPP:F3}**pp (?)")
                            + $" • **`{sc.TotalScore:n0}`**"
                            + (sc.OnlineBestScoreID == null
                                ? ""
                                : $" • [[**Score**]](https://osu.ppy.sh/scores/{ruleset.ShortName}/{sc.OnlineBestScoreID})")
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