using Disqord;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Commons.Structures;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Osu.Commands
{
    internal class ScoresetPagedView : Commons.Structures.Views.PagedView
    {
        public ScoresetPagedView(PageProvider pageProvider) : base(pageProvider) { }
    }

    public partial class Scoreset
    {
        [TextCommand("top", "best")]
        [Description("Show top plays of a player.")]
        public async Task<IDiscordCommandResult> Best(
            [Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset,
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Description("Username to check. Default to your username, if set.")] Username username,
            [Flag("/mod=", "/mod:")][Description("Mods to filter top plays with.")] string mods = "",
            [Flag("#")][Description("Index from the best play. 1 indicates the best play.")] int pos = -1
        )
        {
            var apiClient = APIClientStore.GetClient(server);
            var user = await apiClient.GetUser(username.GetUsername(server)!, ruleset.RulesetInfo);

            if (pos > 0)
            {
                var score = await apiClient.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo, count: 1, offset: pos - 1);
                if (score.Length == 0)
                {
                    return Reply(new LocalEmbed()
                        .WithDescription(
                            $"No top play found for player [{user.Username}]({user.PublicUrl}) at position {pos}."));
                }

                return await SingleScore(score[0]);
            }

            IEnumerable<string> modStrings = mods.Chunk(2).Select(chunk => new string(chunk.ToArray()));
            var modFilters = ModParserService.ResolveMods(ruleset, modStrings);

            var scores = await apiClient.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo);
            var filtered = scores
                .Where(score =>
                {
                    if (modFilters.Length == 0)
                    {
                        return true;
                    }

                    var checkedMods = score.Mods.Select(mod => mod.Acronym).ToList();
                    if (checkedMods.Contains(new OsuModNightcore().Acronym, StringComparer.InvariantCultureIgnoreCase))
                    {
                        checkedMods.Add(new OsuModDoubleTime().Acronym);
                    }

                    return modFilters.All(mod =>
                        checkedMods.Contains(mod.Acronym, StringComparer.InvariantCultureIgnoreCase));
                })
                .ToArray();

            var modText = modFilters.Length > 0 ? $" with mod {string.Join("", modFilters.Select(m => m.Acronym))}" : "";
            var pages = new ArrayPageProvider<APIScore>(
                filtered,
                (_, chunk) => new Page().WithEmbeds(
                    SerializeScoreset(chunk, scoreLink: false)
                        .WithFooter(
                            $"Top plays (all times are UTC){modText}"
                        )
                        .WithAuthor(SerializeAuthorBuilder(user))
                ),
                MaxScorePerPage
            );

            if (pages.PageCount == 0)
            {
                return Reply(new LocalEmbed()
                    .WithDescription(
                        $"No top play found for user [{user.Username}]({user.PublicUrl}) on mode {ruleset.RulesetInfo.Name}{modText}"));
            }

            return Menu(new DefaultTextMenu(new ScoresetPagedView(pages)));
        }
    }
}