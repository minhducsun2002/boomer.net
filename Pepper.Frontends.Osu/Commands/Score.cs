using Disqord;
using Disqord.Bot.Commands;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Structures;
using Pepper.Frontends.Osu.Structures.ParameterAttributes;
using Pepper.Frontends.Osu.Structures.TypeParsers;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Osu.Commands
{
    public class Score : OsuScoreCommand
    {
        public Score(APIClientStore s, BeatmapContextProviderService b, ModParserService p) : base(s, b, p) { }

        [TextCommand("sc", "score", "scores")]
        [Description("View/list scores on a certain map")]
        [OverloadPriority(2)]
        public async Task<IDiscordCommandResult> BeatmapBased(
            [Description("A score URL, a beatmap URL, or a beatmap ID.")][DoNotFill] IBeatmapResolvable beatmapResolvable,
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Remainder][Description("Username to check. Default to your username, if set.")] Username username
        )
        {
            var mapId = beatmapResolvable.BeatmapId;
            var apiClient = APIClientStore.GetClient(server);
            var map = await APIClientStore.GetClient(GameServer.Osu).GetBeatmap(mapId);
            var ruleset = map.GetDefaultRuleset();
            SetBeatmapContext(mapId);

            var user = await apiClient.GetUser(username.GetUsername(server)!, ruleset.RulesetInfo);
            var scores = await apiClient.GetUserBeatmapScores(
                user.Id,
                mapId,
                ruleset.RulesetInfo
            );

            if (scores.Length == 0)
            {
                return Reply(
                    new LocalEmbed()
                        .WithDescription($"No score found for user `{user.Username}` on beatmap " +
                                         $"[**{map.Metadata.Title}** [**{map.BeatmapInfo.DifficultyName}**]](https://osu.ppy.sh/b/{mapId}).")
                );
            }


            var difficulty = map.CalculateDifficulty(ruleset.RulesetInfo.OnlineID);
            HitStatisticsSynthesizer? synthesizer = null;

            return Reply(new LocalEmbed
            {
                Author = SerializeAuthorBuilder(user),
                Title = $"**{map.Beatmap.Metadata.Artist}** - **{map.Beatmap.Metadata.Title}** [{map.BeatmapInfo.DifficultyName}]",
                Url = map.GetOnlineUrl(),
                Description = new BeatmapStatsSerializer(map.BeatmapInfo)
                {
                    ControlPointInfo = map.Beatmap.ControlPointInfo,
                    DifficultyOverwrite = difficulty
                }.Serialize(
                      formatted: true,
                      serializationOptions: StatFilter.Statistics | StatFilter.BPM | StatFilter.StarRating | StatFilter.Length
                )
                + "\n\n"
                + string.Join("\n\n", scores.Select(score =>
                {
                    var localPP = false;
                    var mods = ModParserService.ResolveMods(ruleset, score.Mods);
                    var pp = score.PP;
                    if (!pp.HasValue)
                    {
                        try
                        {
                            synthesizer ??= new HitStatisticsSynthesizer(map.Beatmap.HitObjects.Count);
                            var scoreInfo = new ScoreInfo
                            {
                                Mods = mods,
                                MaxCombo = score.MaxCombo,
                                Accuracy = score.Accuracy,
                                Statistics = synthesizer.Synthesize(ruleset, score.Accuracy)
                            };
                            pp = map.CalculatePerformance(scoreInfo);
                            localPP = true;
                        }
                        catch { /* ignore */ }
                    }

                    return
                        $"[**{score.Rank}**] **{pp:F2}**pp{(localPP ? " (?)" : "")} (**{score.MaxCombo}**x | **{score.Accuracy * 100:F3}**%)"
                        + $" {(score.Perfect ? "(FC)" : "")}"
                        + (mods.Length != 0 ? $"+**{string.Join("", mods.Select(mod => mod.Acronym))}**" : "")
                        + "\n" + SerializeHitStats(score.Statistics, Rulesets[score.RulesetID].RulesetInfo)
                        + $" @ **{SerializeTimestamp(score.Date)}**"
                        + $"\n[**Score link**](https://osu.ppy.sh/scores/{ruleset.ShortName}/{score.OnlineID})";
                })),
                Timestamp = DateTimeOffset.Now,
                Footer = new LocalEmbedFooter
                {
                    Text = "All times are in UTC"
                }
            });
        }

        [TextCommand("sc", "score", "scores", "c", "check")]
        [Description("View/list scores on the latest map posted in the current channel")]
        [OverloadPriority(1)]
        public async Task<IDiscordCommandResult> UserOnly(
            // [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Remainder][Description("Username to check. Default to your username, if set.")] Username username
        )
        {
            var beatmap = await GetBeatmapIdFromContext();
            if (beatmap == null)
            {
                return Reply(
                    "No beatmap was ever sent in this channel. Please use `o!map` with the relevant map link/ID first.");
            }

            return await BeatmapBased(new BeatmapResolvable(beatmap.Value), GameServer.Osu, username);
        }

        // [TextCommand("sc", "score", "scores", "c", "check")]
        // [Description("View/list scores on a certain map (osu! official servers only)")]
        // [OverloadPriority(0)]
        // public async Task<IDiscordCommandResult> ScoreLink([Description("A link to a score.")] string link)
        // {
        //     if (URLParser.CheckScoreUrl(link, out var scoreLink))
        //     {
        //         var (mode, id) = scoreLink;
        //         var sc = await APIClientStore.GetClient(GameServer.Osu).GetScore(
        //             id,
        //             Rulesets
        //                 .First(rulesetCheck => string.Equals(rulesetCheck.ShortName, mode,
        //                     StringComparison.InvariantCultureIgnoreCase))
        //                 .RulesetInfo
        //         );
        //         return await SingleScore(sc);
        //     }
        //
        //     return Reply(string.IsNullOrWhiteSpace(link)
        //         ? "Couldn't determine a beatmap to use in this channel. It's likely that I'm recently restarted."
        //         : "Your score link doesn't look valid.");
        // }
    }
}