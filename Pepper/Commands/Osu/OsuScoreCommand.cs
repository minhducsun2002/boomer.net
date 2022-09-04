using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu;
using Pepper.Structures.External.Osu;
using Pepper.Structures.External.Osu.Extensions;

namespace Pepper.Commands.Osu
{
    public abstract class OsuScoreCommand : BeatmapContextCommand
    {
        protected readonly ModParserService ModParserService;

        protected OsuScoreCommand(APIClientStore s, BeatmapContextProviderService b, ModParserService p) : base(s, b)
        {
            ModParserService = p;
        }

        protected async Task<IDiscordCommandResult> SingleScore(APIScore sc)
        {
            var b = sc.Beatmap!;
            var workingBeatmap = await APIClientStore.GetClient(GameServer.Osu).GetBeatmap(b.OnlineID);
            var ruleset = Rulesets[sc.RulesetID];

            var mods = ModParserService.ResolveMods(ruleset, sc.Mods.Select(mod => mod.Acronym));
            var difficulty = workingBeatmap.CalculateDifficulty(sc.RulesetID, true, mods);

            SetBeatmapContext(workingBeatmap.BeatmapInfo.OnlineID);

            b.StarRating = difficulty.StarRating;
            double? pp = sc.PP, fullComboPP = pp;
            var calculated = false;

            if (!pp.HasValue)
            {
                pp = workingBeatmap.CalculatePerformance(
                    rulesetOverwrite: ruleset,
                    score: new ScoreInfo { Mods = mods, MaxCombo = difficulty.MaxCombo, Accuracy = sc.Accuracy }
                        .WithRulesetID(sc.RulesetID)
                        .WithStatistics(sc.Statistics)
                );
                calculated = true;
            }

            if (!sc.Perfect)
            {
                var statistics = new HitStatisticsSynthesizer(sc.Statistics.Values.Sum())
                    .Synthesize(ruleset, sc.Accuracy);
                var fcScore = new ScoreInfo { Mods = mods, MaxCombo = difficulty.MaxCombo, Accuracy = sc.Accuracy, Statistics = statistics }
                    .WithRulesetID(sc.RulesetID);
                fullComboPP = workingBeatmap.CalculatePerformance(fcScore, ruleset);
            }

            int hitCounts, totalHitCounts = workingBeatmap.Beatmap.HitObjects.Count;
            {
                var temporaryScore = new ScoreInfo().WithRulesetID(sc.RulesetID).WithStatistics(sc.Statistics);
                hitCounts = temporaryScore.Statistics.Values.Sum();
            }

            var footer = "";
            var version = SharedConstants.OsuVersion?.ToString();
            if (totalHitCounts > hitCounts)
            {
                footer = $"{hitCounts / (float) totalHitCounts * 100:F2}% completed";
            }
            if (version is not null && (calculated || !sc.Perfect))
            {
                footer += (string.IsNullOrEmpty(footer) ? "" : "  •  ") + $"osu!lazer {version}";
            }

            var embed = new LocalEmbed
            {
                Author = SerializeAuthorBuilder(sc.User),
                Title = $"[**{sc.Rank}**] {b.Metadata.Artist} - {b.Metadata.Title} [{b.DifficultyName}]"
                        + (mods.Length != 0 ? "+" + string.Join("", mods.Select(mod => mod.Acronym)) : ""),
                Url = workingBeatmap.GetOnlineUrl(),
                ThumbnailUrl =
                    $"https://b.ppy.sh/thumb/{workingBeatmap.BeatmapSetInfo?.OnlineID ?? b.OnlineBeatmapSetID}l.jpg",
                Timestamp = sc.Date,
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Statistics",
                        Value =
                            $"**{sc.MaxCombo}**x/**{difficulty.MaxCombo}**x • [{SerializeHitStats(sc.Statistics, ruleset.RulesetInfo)}] • **{sc.Accuracy * 100:F3}**%"
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
                        Value = new BeatmapStatsSerializer(workingBeatmap.BeatmapInfo)
                        {
                            Mods = mods,
                            ControlPointInfo = workingBeatmap.Beatmap.ControlPointInfo,
                            DifficultyOverwrite = difficulty
                        }.Serialize(
                            formatted: true,
                            serializationOptions: StatFilter.Statistics |
                                                  StatFilter.BPM |
                                                  StatFilter.StarRating |
                                                  (true ? StatFilter.Length : 0)
                        )
                    }
                },
            };

            if (!string.IsNullOrEmpty(footer))
            {
                embed.Footer = new LocalEmbedFooter().WithText(footer);
            }

            if (sc.User.Id == 16212851 || Context.Author.Id == 490107873834303488)
            {
                var baseMsg = "đm xoài, ngu";
                if ((workingBeatmap.BeatmapSetInfo?.OnlineID ?? b.OnlineBeatmapSetID) == 497942)
                {
                    baseMsg += ", farm harebare fanfare lắm vl";
                }

                var isNc = false;
                if (mods.Any(m => m.Acronym.ToUpperInvariant() == "DT" || (isNc = m.Acronym.ToUpperInvariant() == "NC")) &&
                    mods.Any(m => m.Acronym.ToUpperInvariant() == "HD"))
                {
                    baseMsg += " lại còn farm HD" + (isNc ? " rồi còn bày đặt NC" : "DT");
                }

                return Reply(baseMsg, embed);
            }

            return Reply(embed);
        }
    }
}