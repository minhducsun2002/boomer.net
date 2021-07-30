using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Pepper.Services.Osu;
using Pepper.Services.Osu.API;
using Pepper.Structures.External.Osu.Extensions;

namespace Pepper.Commands.Osu
{
    public abstract class OsuScoreCommand : OsuCommand
    {
        protected OsuScoreCommand(APIService service) : base(service) {}

        public async Task<DiscordCommandResult> SingleScore(APILegacyScoreInfo sc)
        {
            var b = sc.Beatmap!;
            var workingBeatmap = await APIService.GetBeatmap(b.OnlineBeatmapID!.Value);
            var s = workingBeatmap.BeatmapInfo.BeatmapSet;
            var ruleset = Rulesets[sc.OnlineRulesetID];

            double? pp = sc.PP, fullComboPP = sc.PP;
            var calculated = false;
            
            var mods = ResolveMods(ruleset, sc.Mods);
            var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods);
            var maxCombo = difficulty.MaxCombo;
            
            if (!pp.HasValue || !sc.Perfect)
            {
                if (!pp.HasValue)
                {
                    var score = new ScoreInfo { Mods = mods, MaxCombo = sc.MaxCombo, Accuracy = sc.Accuracy };
                    score.SetStatistics(sc.Statistics);
                    var performanceCalculator = GetPerformanceCalculator(sc.OnlineRulesetID, difficulty, score);
                    pp = performanceCalculator.Calculate();
                    calculated = true;                    
                }

                if (!sc.Perfect)
                {
                    var fcScore = new ScoreInfo { Mods = mods, MaxCombo = maxCombo, Accuracy = sc.Accuracy };
                    fcScore.SetStatistics(sc.Statistics);
                    fcScore.SetCount300((int) (fcScore.GetCount300() + fcScore.GetCountMiss())!);
                    fcScore.SetCountMiss(0);
                    var performanceCalculator =
                        GetPerformanceCalculator(sc.OnlineRulesetID, difficulty, fcScore);
                    fullComboPP = performanceCalculator.Calculate();
                }
            }

            b.StarDifficulty = difficulty.StarRating;

            return Reply(new LocalEmbed
            {
                Author = SerializeAuthorBuilder(sc.User),
                Title = $"[**{sc.Rank}**] {b.Metadata.Artist} - {b.Metadata.Title} [{b.Version}]"
                        + (sc.Mods.Any() ? "+" + string.Join("", sc.Mods) : ""),
                Url = $"https://osu.ppy.sh/beatmapsets/{s.OnlineBeatmapSetID}#{ruleset.ShortName}/{b.OnlineBeatmapID}",
                ThumbnailUrl = $"https://assets.ppy.sh/beatmaps/{s.OnlineBeatmapSetID}/covers/list@2x.jpg?",
                Timestamp = sc.Date,
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Statistics",
                        Value =
                            $"**{sc.MaxCombo}**x/**{maxCombo}**x • [{SerializeHitStats(sc.Statistics)}] • **{sc.Accuracy * 100:F3}**%"
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
                        Value = SerializeBeatmapStats(b, difficulty)
                    }
                }
            });
        }
    }
}