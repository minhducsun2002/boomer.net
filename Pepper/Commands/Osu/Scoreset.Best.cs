using System;
using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Qmmands;
using PagedView = Pepper.Structures.PagedView;

namespace Pepper.Commands.Osu
{
    internal class ScoresetPagedView : PagedView
    {
        public ScoresetPagedView(PageProvider pageProvider) : base(pageProvider) { }
    }

    public partial class Scoreset
    {
        [Command("top", "best")]
        [Description("Show top plays of a player.")]
        public async Task<DiscordCommandResult> Best(
            [Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset,
            [Description("Username to check. Default to your username, if set.")] Username username,
            [Flag("-")][Description("Game server to check. Default to osu! official servers.")] GameServer server,
            [Flag("/mod=", "/mod:")][Description("Mods to filter top plays with.")] string mods = "",
            [Flag("#")][Description("Index from the best play. 1 indicates the best play.")] int pos = -1
        )
        {
            var apiClient = APIClientStore.GetClient(server);
            var user = await apiClient.GetUser(username.GetUsername(server)!, ruleset.RulesetInfo);

            if (pos > 0)
            {
                var score = await apiClient.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo, 1, pos - 1);
                if (score.Length == 0)
                {
                    return Reply(new LocalEmbed()
                        .WithDescription(
                            $"No top play found for player [{user.Username}]({user.PublicUrl}) at position {pos}."));
                }

                return await SingleScore(score[0]);
            }

            var modFilters = ResolveMods(
                ruleset,
                mods.Chunk(2).Select(chunk => new string(chunk.ToArray()))
            );

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

            var pages = new ArrayPageProvider<APIScoreInfo>(
                filtered,
                (_, chunk) => new Page().WithEmbeds(
                    SerializeScoreset(chunk, scoreLink: false)
                        .WithFooter($"Top plays (all times are UTC)")
                        .WithAuthor(SerializeAuthorBuilder(user))
                ),
                MaxScorePerPage
            );

            if (pages.PageCount == 0)
            {
                return Reply(new LocalEmbed()
                    .WithDescription(
                        $"No top play found for user [{user.Username}]({user.PublicUrl}) on mode {ruleset.RulesetInfo.Name}"));
            }

            return View(new ScoresetPagedView(pages));
        }
    }
}