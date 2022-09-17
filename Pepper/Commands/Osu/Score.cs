using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Bot.Commands;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Services.Osu;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Metadata;
using Pepper.Structures.External.Osu;
using Pepper.Utilities.Osu;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commands.Osu
{
    public class Score : OsuScoreCommand
    {
        public Score(APIClientStore s, BeatmapContextProviderService b, ModParserService p) : base(s, b, p) { }

        [TextCommand("sc", "score", "scores", "c", "check")]
        [Description("View/list scores on a certain map")]
        [OverloadPriority(2)]
        public async Task<IDiscordCommandResult> BeatmapBased(
            [Description("A score URL, a beatmap URL, or a beatmap ID.")] IBeatmapResolvable beatmapResolvable,
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
                                              MaxCombo = difficulty.MaxCombo,
                                              Accuracy = score.Accuracy,
                                              Statistics = synthesizer.Synthesize(ruleset, score.Accuracy)
                                          };
                                          pp = (float) map.CalculatePerformance(scoreInfo);
                                          localPP = true;
                                      }
                                      catch { /* ignore */ }
                                  }

                                  return
                                      $"[**{score.Rank}**] **{pp}**pp{(localPP ? " (?)" : "")} (**{score.MaxCombo}**x | **{score.Accuracy:F3}**%)"
                                      + $" {(score.Perfect ? "(FC)" : "")}"
                                      + (mods.Length != 0 ? $"+**{string.Join("", mods.Select(mod => mod.Acronym))}**" : "")
                                      + "\n" + SerializeHitStats(score.Statistics, Rulesets[score.RulesetID].RulesetInfo)
                                      + $" @ **{SerializeTimestamp(score.Date, false)}**"
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
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Remainder][Description("Username to check. Default to your username, if set.")] Username username
        )
        {
            var beatmap = BeatmapContext.GetBeatmap(Context.ChannelId.ToString());
            if (beatmap == null)
            {
                return Reply(
                    "No beatmap was ever sent in this channel. Please use `o!map` with the relevant map link/ID first.");
            }

            return await BeatmapBased(new BeatmapResolvable(beatmap.Value), server, username);
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