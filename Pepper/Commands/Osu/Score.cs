using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Pepper.Utilities.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class Score : OsuScoreCommand
    {
        public Score(APIClientStore s, BeatmapContextProviderService b, ModParserService p) : base(s, b, p) { }

        [Command("sc", "score", "scores", "c", "check")]
        [Description("View/list scores on a certain map")]
        [Priority(1)]
        public async Task<DiscordCommandResult> BeatmapBased(
            [Description("A score URL, a beatmap URL, or a beatmap ID.")] IBeatmapResolvable beatmapResolvable,
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Remainder][Description("Username to check. Default to your username, if set.")] Username? username = null
        )
        {
            var mapId = beatmapResolvable.BeatmapId;
            if (username != null)
            {
                var apiClient = APIClientStore.GetClient(server);
                var map = await APIClientStore.GetClient(GameServer.Osu).GetBeatmap(mapId);
                var ruleset = map.GetDefaultRuleset();
                SetBeatmapContext(mapId);

                var user = await apiClient.GetUser(username.GetUsername(server)!, ruleset.RulesetInfo);
                var scores = await apiClient.GetLegacyBeatmapScores(
                    user.Id,
                    mapId,
                    ruleset.RulesetInfo
                );

                if (scores.Count == 0)
                {
                    return Reply($"No score found on that beatmap for user `{user.Username}`.");
                }

                var difficulty = map.CalculateDifficulty(ruleset.RulesetInfo.OnlineID);

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
                            {
                                try
                                {
                                    var scoreInfo = new ScoreInfo { Mods = mods, MaxCombo = score.MaxCombo!.Value, Accuracy = score.Accuracy };
                                    pp = (float) map.CalculatePerformance(scoreInfo);
                                    localPP = true;
                                }
                                catch { /* ignore */ }
                            }

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

            if (username == null)
            {
                throw new ArgumentException("No username is passed!");
            }

            throw new ArgumentException("A valid beatmap-resolvable must be passed!");
        }

        [Command("sc", "score", "scores", "c", "check")]
        [Description("View/list scores on a certain map (osu! official servers only)")]
        [Priority(0)]
        public async Task<DiscordCommandResult> ScoreLink([Description("A link to a score.")] string link)
        {
            if (URLParser.CheckScoreUrl(link, out var scoreLink))
            {
                var (mode, id) = scoreLink;
                var sc = await APIClientStore.GetClient(GameServer.Osu).GetScore(
                    id,
                    Rulesets
                        .First(rulesetCheck => string.Equals(rulesetCheck.ShortName, mode,
                            StringComparison.InvariantCultureIgnoreCase))
                        .RulesetInfo
                );
                return await SingleScore(sc);
            }

            return Reply(string.IsNullOrWhiteSpace(link)
                ? "Couldn't determine a beatmap to use in this channel. It's likely that I'm recently restarted."
                : "Your score link doesn't look valid.");
        }
    }
}