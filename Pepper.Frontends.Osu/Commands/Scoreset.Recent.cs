using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Structures;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Osu.Commands
{
    public partial class Scoreset
    {
        [TextCommand("recent", "recentplay", "rp")]
        [Description("Show recent plays of a player.")]
        public async Task<IDiscordCommandResult> Recent(
            [Flag("/")][Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            // [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Description("Username to check. Default to your username, if set.")] Username username
        )
        {
            var rulesetInfo = ruleset.RulesetInfo;
            var apiClient = APIClientStore.GetClient(server);
            var user = await apiClient.GetUser(username.GetUsername(server)!, rulesetInfo);

            var scores = await apiClient.GetUserScores(user.Id, ScoreType.Recent, rulesetInfo, true, 100);

            var pages = new ArrayPageProvider<APIScore>(
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

            return Menu(new DefaultTextMenu(new ScoresetPagedView(pages)));
        }

        [TextCommand("rs")]
        [Description("Show the most recent play of a player.")]
        public async Task<IDiscordCommandResult> MostRecent(
            [Flag("/")][Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            // [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
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

            if (user.Id == 16212851 || Context.Author.Id == 490107873834303488)
            {
                return Reply("đm xoài ngu", new LocalEmbed()
                    .WithDescription(
                        $"No recent play found for user [{user.Username}]({user.PublicUrl}) on mode {rulesetInfo.Name} at position #{pos}."
                    ));
            }

            return Reply(new LocalEmbed()
                .WithDescription(
                    $"No recent play found for user [{user.Username}]({user.PublicUrl}) on mode {rulesetInfo.Name} at position #{pos}."
                ));
        }
    }
}