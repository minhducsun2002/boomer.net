using Disqord;
using Disqord.Bot.Commands;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu;
using Pepper.Commons.Structures;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Pepper.Frontends.Osu.Structures;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Osu.Commands
{
    public class User : OsuCommand
    {
        public User(APIClientStore apiClientStore) : base(apiClientStore) { }

        [TextCommand("user", "u")]
        [Description("Show statistics of an osu! player.")]
        public async Task<IDiscordCommandResult> Exec(
            [Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset,
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Remainder][Description("Username to check. Default to your username, if set.")] Username username)
        {
            var apiClient = APIClientStore.GetClient(server);
            var user = await apiClient.GetUser(username.GetUsername(server)!, ruleset.RulesetInfo);
            var color = await apiClient.GetUserColor(user);
            var scores = await apiClient.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo, count: 1);

            var stats = user.Statistics;
            var grades = stats.GradesCount;
            var playTime = TimeSpan.FromSeconds(stats.PlayTime!.Value);
            var earthEmoji = ResolveEarthEmoji(user.CountryCode.ToString());

            var embed = new LocalEmbed
            {
                Title = user.Username,
                Url = user.PublicUrl,
                ThumbnailUrl = user.AvatarUrl,
                Description = (
                    stats.GlobalRank.Equals(default)
                        ? "Unranked"
                        : $"**{stats.PP}**pp ({earthEmoji} #**{stats.GlobalRank}** | :flag_{user.CountryCode.ToString().ToLowerInvariant()}: #**{stats.CountryRank}**)"
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
                Footer = new LocalEmbedFooter { Text = "Joined" },
                Timestamp = user.JoinDate,
                Color = new Color(color.R, color.G, color.B)
            };

            if (scores.Length > 0)
            {
                var score = scores[0];
                var map = score.Beatmap!;
                var mapset = map.Metadata;
                embed.AddField(new LocalEmbedField
                {
                    Name = "Best performance",
                    Value = $"[**{score.Rank}**] **{score.PP}**pp "
                            + $"(**{(score.Accuracy * 100):F3}**% | **{score.MaxCombo}**x)" + (score.Perfect ? " (FC)" : "")
                            + $"\n[{mapset.Artist} - {mapset.Title} [{map.DifficultyName}]](https://osu.ppy.sh/beatmaps/{map.OnlineID})"
                            + (score.Mods.Any() ? $"+{string.Join("", score.Mods.Select(m => m.Acronym))}" : "")
                            + "\n"
                            + new BeatmapStatsSerializer(map).Serialize(
                                formatted: true,
                                serializationOptions: StatFilter.StarRating | StatFilter.Statistics | StatFilter.BPM | StatFilter.Length
                            )
                });
            }

            return Reply(embed);
        }
    }
}