using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu.API;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public partial class Scoreset
    {
        [Command("recent", "recentplay", "rp")]
        [Description("Show recent plays of a player.")]
        public async Task<DiscordCommandResult> Recent(
            [Flag("/")][Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            [Description("Username to check. Default to your username, if set.")] Username username
        )
        {
            var rulesetInfo = ruleset.RulesetInfo;
            var (user, _, _) = await APIService.GetUser(username, rulesetInfo);

            var scores = await APIService.GetUserScores(user.Id, ScoreType.Recent, rulesetInfo);

            var pages = new ArrayPageProvider<APILegacyScoreInfo>(
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
                        $@"No recent play found for user [{user.Username}](https://osu.ppy.sh/users/{user.Id}) on mode {rulesetInfo.Name}"
                    ));
            }

            return View(new ScoresetPagedView(pages));
        }

        [Command("rs")]
        [Description("Show the most recent play of a player.")]
        public async Task<DiscordCommandResult> MostRecent(
            [Flag("/")][Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            [Description("Username to check. Default to your username, if set.")] Username username,
            [Flag("#")][Description("Index from the latest play. 1 indicates the latest.")] int pos = 1)
        {
            var rulesetInfo = ruleset.RulesetInfo;
            var (user, _, _) = await APIService.GetUser(username, rulesetInfo);

            var scores = await APIService.GetLegacyUserRecentScores(user.Id, rulesetInfo, pos);
            if (scores.ElementAtOrDefault(pos - 1) != default)
            {
                return await SingleScore(user, scores[pos - 1]);
            }

            return Reply(new LocalEmbed()
                .WithDescription(
                    $"No recent play found for user [{user.Username}](https://osu.ppy.sh/users/{user.Id}) on mode {rulesetInfo.Name} at position #{pos}."
                ));
        }
    }
}