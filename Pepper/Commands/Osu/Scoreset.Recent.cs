using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Osu.APIClients.Ripple;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public partial class Scoreset
    {
        [Command("recent", "recentplay", "rp")]
        [Description("Show recent plays of a player.")]
        public async Task<DiscordCommandResult> Recent(
            [Flag("/")][Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Description("Username to check. Default to your username, if set.")] Username username
        )
        {
            var rulesetInfo = ruleset.RulesetInfo;
            var apiClient = APIClientStore.GetClient(server);
            var user = await apiClient.GetUser(username.GetUsername(server)!, rulesetInfo);

            var scores = await apiClient.GetUserScores(user.Id, ScoreType.Recent, rulesetInfo, true, 100);

            var pages = new ArrayPageProvider<APIScoreInfo>(
                scores,
                (_, chunk) => new Page().WithEmbeds(
                    SerializeScoreset(chunk, scoreLink: false)
                        .WithFooter($"Recent plays (all times are UTC)")
                        .WithAuthor(SerializeAuthorBuilder(user))
                ),
                MaxScorePerPage
            );

            if (pages.PageCount == 0)
            {
                return Reply(new LocalEmbed()
                    .WithDescription(
                        $@"No recent play found for user [{user.Username}]({user.PublicUrl}) on mode {rulesetInfo.Name}"
                    ));
            }

            return Menu(new DefaultMenu(new ScoresetPagedView(pages)));
        }

        [Command("rs")]
        [Description("Show the most recent play of a player.")]
        public async Task<DiscordCommandResult> MostRecent(
            [Flag("/")][Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Description("Username to check. Default to your username, if set.")] Username username,
            [Flag("#")][Description("Index from the latest play. 1 indicates the latest.")] int pos = 1)
        {
            var rulesetInfo = ruleset.RulesetInfo;
            var apiClient = APIClientStore.GetClient(server);
            var user = await apiClient.GetUser(username.GetUsername(server)!, rulesetInfo);

            var scores = await apiClient.GetUserScores(user.Id, ScoreType.Recent, ruleset.RulesetInfo, true, 1, pos - 1);
            if (scores.Length != 0)
            {
                scores[0].User = user;
                return await SingleScore(scores[0]);
            }

            return Reply(new LocalEmbed()
                .WithDescription(
                    $"No recent play found for user [{user.Username}]({user.PublicUrl}) on mode {rulesetInfo.Name} at position #{pos}."
                ));
        }
    }
}