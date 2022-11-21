using Disqord;
using Disqord.Bot.Commands;
using osu.Game.Scoring;
using Pepper.Commons.Extensions;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Structures;
using Pepper.Frontends.Osu.Structures.Extensions;

namespace Pepper.Frontends.Osu.Commands
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
                    score: new ScoreInfo { Mods = mods, MaxCombo = sc.MaxCombo, Accuracy = sc.Accuracy, Statistics = sc.Statistics }
                        .WithRulesetID(sc.RulesetID)
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
                var temporaryScore = new ScoreInfo
                {
                    Statistics = sc.Statistics
                }
                    .WithRulesetID(sc.RulesetID);
                hitCounts = temporaryScore.Statistics.Values.Sum();
            }

            var footer = "";
            var version = SharedConstants.OsuVersion?.ToString();
            if (totalHitCounts > hitCounts)
            {
                var time = workingBeatmap.Beatmap.HitObjects[hitCounts - 1].StartTime;
                footer = $"{hitCounts / (float) totalHitCounts * 100:F2}% completed (around {time.SerializeAsMiliseconds()})";
            }
            if (version is not null && (calculated || !sc.Perfect))
            {
                footer += (string.IsNullOrEmpty(footer) ? "" : "  •  ") + $"osu!lazer {version}";
            }

            var embed = new LocalEmbed
            {
                Author = SerializeAuthorBuilder(sc.User!),
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
                                                  StatFilter.Length
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