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
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu.API;
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
            [Flag("/mod=", "/mod:")][Description("Mods to filter top plays with.")] string mods = "",
            [Flag("#")][Description("Index from the best play. 1 indicates the best play.")] int pos = -1
        )
        {
            var (user, _, _) = await APIService.GetUser(username, ruleset.RulesetInfo);

            if (pos > 0)
            {
                var score = await APIService.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo, 1, pos - 1);
                if (score.Length == 0)
                {
                    return Reply(new LocalEmbed()
                        .WithDescription(
                            $"No top play found for player [{user.Username}](https://osu.ppy.sh/users/{user.Id}) at position {pos}."));
                }

                return await SingleScore(score[0]);
            }

            var modFilters = ResolveMods(
                ruleset,
                mods.Chunk(2).Select(chunk => new string(chunk.ToArray())).ToArray()
            );

            var scores = await APIService.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo);
            var filtered = scores
                .Where(score =>
                {
                    if (modFilters.Length == 0)
                    {
                        return true;
                    }

                    var checkedMods = score.Mods!;
                    if (checkedMods.Contains(new OsuModNightcore().Acronym, StringComparer.InvariantCultureIgnoreCase))
                    {
                        checkedMods = score.Mods.Append(new OsuModDoubleTime().Acronym).ToArray();
                    }

                    return modFilters.All(mod =>
                        checkedMods.Contains(mod.Acronym, StringComparer.InvariantCultureIgnoreCase));
                })
                .ToArray();

            var pages = new ArrayPageProvider<APILegacyScoreInfo>(
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
                        $"No top play found for user [{user.Username}](https://osu.ppy.sh/users/{user.Id}) on mode {ruleset.RulesetInfo.Name}"));
            }

            return View(new ScoresetPagedView(pages));
        }
    }
}