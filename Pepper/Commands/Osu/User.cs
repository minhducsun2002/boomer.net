using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    [NotFoundHandler]
    public class User : OsuCommand
    {
        public User(IAPIClient service) : base(service) { }

        [Command("user", "u")]
        [Description("Show statistics of an osu! player.")]
        public async Task<DiscordCommandResult> Exec(
            [Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset,
            [Remainder][Description("Username to check. Default to your username, if set.")] Username username)
        {
            var user = await APIService.GetUser(username, ruleset.RulesetInfo);
            var color = await APIService.GetUserColor(user);
            var scores = await APIService.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo, 1);

            var stats = user.Statistics;
            var grades = stats.GradesCount;
            var playTime = new TimeSpan((stats.PlayTime ?? 0) * (long) 1e7);
            var earthEmoji = ResolveEarthEmoji(user.Country.FlagName);

            var embed = new LocalEmbed
            {
                Title = user.Username,
                Url = $"https://osu.ppy.sh/users/{user.Id}",
                ThumbnailUrl = user.AvatarUrl,
                Description = (
                    stats.GlobalRank.Equals(default)
                        ? "Unranked"
                        : $"**{stats.PP}**pp ({earthEmoji} #**{stats.GlobalRank}** | :flag_{user.Country.FlagName.ToLowerInvariant()}: #**{stats.CountryRank}**)"
                    ) + $".\n**{stats.Accuracy:F3}**% accuracy - **{stats.MaxCombo}**x max combo.",
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = $"Play ranks",
                        Value = $"**{grades.SSPlus}** XH | **{grades.SS}** X\n**{grades.SPlus}** SH | **{grades.S}** S\n**{grades.A}** A",
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Play count",
                        Value = $"{stats.PlayCount} times\n"
                                + $"{playTime.Days / 7}w {playTime.Days % 7}d {playTime.Hours}h {playTime.Minutes}m {playTime.Seconds}s",
                        IsInline = true
                    },
                },
                Footer = new LocalEmbedFooter { Text = $"Joined {SerializeTimestamp(user.JoinDate)}." },
                Color = new Color(color.R, color.G, color.B)
            };

            if (scores.Length > 0)
            {
                var score = scores[0];
                var map = score.Beatmap;
                var mapset = map.Metadata;
                embed.Fields.Add(new LocalEmbedField
                {
                    Name = "Best performance",
                    Value = $"[**{score.Rank}**] **{score.PP}**pp "
                            + $"(**{(score.Accuracy * 100):F3}**% | **{score.MaxCombo}**x)" + (score.Perfect ? " (FC)" : "")
                            + $"\n[{mapset.Artist} - {mapset.Title} [{map.DifficultyName}]](https://osu.ppy.sh/beatmaps/{map.OnlineID})"
                            + (score.Mods.Any() ? $"+{string.Join("", score.Mods)}" : "")
                            + $"\n{SerializeBeatmapStats(map)}"
                });
            }

            return Reply(embed);
        }


        private class NotFoundHandlerAttribute : Attribute, ICommandExecutionFailureFormatter
        {
            // handle 404s
            public LocalMessage? FormatFailure(DiscordCommandContext context, CommandExecutionFailedResult commandExecutionFailedResult)
            {
                var exception = commandExecutionFailedResult.Exception;
                if (exception is HttpRequestException { StatusCode: HttpStatusCode.NotFound })
                {
                    var username = context.Arguments.OfType<Username>().First();
                    return new LocalMessage().WithContent(
                        $"User **{username}** isn't found. Either the user doesn't exist, or they're under a restriction."
                    );
                }

                return null;
            }
        }
    }
}