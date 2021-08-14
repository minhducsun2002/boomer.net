using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using OsuSharp;
using Pepper.Services.Osu;
using Pepper.Structures.External.Osu;
using Pepper.Utilities.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class Score : OsuScoreCommand
    {
        public Score(APIService s, BeatmapContextProviderService b) : base(s, b) {}

        [Command("sc", "score", "scores")]
        [Description("View/list scores on a certain map")]
        public async Task<DiscordCommandResult> Exec(
            [Description("A score URL, a beatmap URL, or a beatmap ID.")] string link = "",
            [Remainder] [Description("Username to check. Default to your username, if set. Ignored if a score link is passed.")] Username? username = null
        )
        {
            if (string.IsNullOrWhiteSpace(link)) link = GetBeatmapContext()?.ToString() ?? "";
            
            if (URLParser.CheckScoreUrl(link, out var scoreLink))
            {
                var (mode, id) = scoreLink;
                var sc = await APIService.GetScore(
                    id,
                    Rulesets
                        .First(rulesetCheck => string.Equals(rulesetCheck.ShortName, mode,
                            StringComparison.InvariantCultureIgnoreCase))
                        .RulesetInfo
                );
                return await SingleScore(sc);
            }

            int? mapId;
            string? gameMode = null;
            if (!int.TryParse(link, out var validMapId))
                URLParser.CheckMapUrl(link, out gameMode, out mapId, out _);
            else
                mapId = validMapId;
            
            if (mapId.HasValue && username != null)
            {
                var map = await APIService.GetBeatmap(mapId.Value);
                SetBeatmapContext(mapId.Value);
                
                var parsed = await Context.Command.Service.GetTypeParser<Ruleset>()!
                    .ParseAsync(default, gameMode ?? RulesetTypeParser.SupportedRulesets[map.BeatmapInfo.RulesetID].ShortName, Context);
                var ruleset = parsed.Value;
                var (user, _, _) = await APIService.GetUser(username, ruleset.RulesetInfo);
                var scores = await APIService.GetLegacyBeatmapScores(
                    user.Id,
                    mapId.Value,
                    parsed.Value.RulesetInfo
                );

                if (scores.Count == 0) return Reply($"No score found on that beatmap for user `{username.Content}`.");

                var difficultyCalculator = ruleset.CreateDifficultyCalculator(map);
                var difficulty = difficultyCalculator.Calculate();

                return Reply(new LocalEmbed
                {
                    Author = SerializeAuthorBuilder(user),
                    Title = $"**{map.Beatmap.Metadata.Artist}** - **{map.Beatmap.Metadata.Title}** [{map.BeatmapInfo.Version}]",
                    Url = map.GetOnlineUrl(),
                    Description = $"{SerializeBeatmapStats(map.BeatmapInfo, difficulty, map.Beatmap.ControlPointInfo)}\n\n"
                        + string.Join("\n\n", scores.Select(score =>
                        {
                            var localPP = false;
                            var mods = ruleset.ConvertFromLegacyMods((LegacyMods) score.Mods)!.ToArray();
                            var pp = score.PerformancePoints;
                            if (!pp.HasValue)
                                try
                                {
                                    var scoreInfo = new ScoreInfo
                                        {Mods = mods, MaxCombo = score.MaxCombo!.Value, Accuracy = score.Accuracy};
                                    var performanceCalculator = GetPerformanceCalculator(
                                        (int) score.GameMode,
                                        difficultyCalculator.Calculate(mods),
                                        scoreInfo
                                    );
                                    pp = (float) performanceCalculator.Calculate();
                                    localPP = true;
                                }
                                catch { /* ignore */ }

                            return
                                $"[**{score.Rank}**] **{pp}**pp{(localPP ? " (?)" : "")} (**{score.MaxCombo}**x | **{score.Accuracy:F3}**%)"
                                + $" {(score.Perfect ? "(FC)" : "")}"
                                + (mods.Length != 0 ? $"+**{string.Join("", mods.Select(mod => mod.Acronym))}**" : "")
                                + $"\n[**{score.Count300}**/**{score.Count100}**/**{score.Count50}**/**{score.Miss}**]"
                                + $" @ **{SerializeTimestamp(score.Date!.Value, false)}**"
                                + $"\n[**Score link**](https://osu.ppy.sh/scores/{ruleset.ShortName}/{score.ScoreId})";
                        })),
                    Timestamp = DateTimeOffset.Now,
                    Footer = new LocalEmbedFooter
                    {
                        Text = "All times are in UTC"
                    }
                });
            }
            
            if (username == null) throw new ArgumentException("No username is passed!");
            throw new ArgumentException("A valid score link must be passed!");
        }
    }
}