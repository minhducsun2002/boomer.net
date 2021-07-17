using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Rulesets;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class User : OsuCommand
    {
        [Command("user", "u")]
        [Description("Show statistics of an osu! player.")]
        public async Task<DiscordCommandResult> Exec(
            [Flag("/")] [Description("Game mode to check. Default to osu!.")] Ruleset ruleset,
            [Remainder] [Description("Username to check. Default to your username, if set.")] Username username)
        {
            var (user, scores, color) = await ApiService.GetUser(username, ruleset.RulesetInfo);
            var stats = user.Statistics;
            var grades = stats.GradesCount;
            var playTime = new TimeSpan((stats.PlayTime ?? 0) * (long) 1e7);
            var countryCode = user.Country.FlagName;
            var earthEmoji = ResolveEarthEmoji(AJ.Code.Country.GetCountryInfoForAlpha2Code(countryCode)!.ContinentCode);

            var embed = new LocalEmbed
            {
                Title = user.Username,
                Url = $"https://osu.ppy.sh/users/{user.Id}",
                ThumbnailUrl = user.AvatarUrl,
                Description = (
                    stats.GlobalRank.Equals(default)
                        ? "Unranked"
                        : $"**{stats.PP}**pp ({earthEmoji} #**{stats.GlobalRank}** | :flag_{countryCode.ToLowerInvariant()}: #**{stats.CountryRank}**)"
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
                            + $"\n[{mapset.Artist} - {mapset.Title} [{map.Version}]](https://osu.ppy.sh/beatmaps/{map.OnlineBeatmapID})"
                            + (score.Mods.Length > 0 ? $"+{string.Join("", score.Mods)}" : "")
                            + $"\n{SerializeBeatmapStats(map)}"
                });
            }

            return Reply(embed);
        }
    }
}