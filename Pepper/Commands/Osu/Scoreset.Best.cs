using System;
using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Discord;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu.Mods;
using Pepper.Structures.Commands;
using Pepper.Structures.Commands.Result;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public partial class Scoreset
    {
        [Command("top", "best")]
        [Description("Show top plays of a player.")]
        public async Task<EmbedResult> Best(
            [Flag("/")] [Description("Game mode to check. Default to osu!.")] Ruleset ruleset,
            [Flag("/mod=", "/mod:")] [Description("Mods to filter top plays with.")] string mods,
            [Description("Username to check. Default to your username, if set.")] Username username,
            [Flag("#")] [Description("Index from the best play. 1 indicates the best play.")] int pos = -1
        )
        {
            var (user, _, _) = await ApiService.GetUser(username, ruleset.RulesetInfo);

            if (pos > 0)
            {
                var score = await ApiService.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo, 1, pos - 1);
                if (score.Length == 0)
                    return new EmbedResult
                    {
                        DefaultEmbed = new EmbedBuilder()
                            .WithDescription(
                                $"No top play found for player [{user.Username}](https://osu.ppy.sh/users/{user.Id}) at position {pos}.")
                            .Build()
                    };
                return await SingleScore(score[0]);
            }

            var modFilters = ResolveMods(
                ruleset,
                mods.Chunk(2).Select(chunk => new string(chunk.ToArray())).ToArray()
            );
            
            var scores = await ApiService.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo);
            var chunks = scores
                .Where(score =>
                {
                    if (modFilters.Length == 0) return true;
                    var checkedMods = score.Mods!;
                    if (checkedMods.Contains(new OsuModNightcore().Acronym, StringComparer.InvariantCultureIgnoreCase))
                        checkedMods = score.Mods.Append(new OsuModDoubleTime().Acronym).ToArray();
                    return modFilters.All(mod =>
                        checkedMods.Contains(mod.Acronym, StringComparer.InvariantCultureIgnoreCase));
                })
                .Chunk(MaxScorePerPage).ToArray();
            return new EmbedResult
            {
                Embeds = chunks.Select((embed, index) => SerializeScoreset(embed)
                        .WithFooter($"Top plays - page {index + 1}/{chunks.Length}")
                        .WithAuthor(SerializeAuthorBuilder(user))
                        .Build())
                    .ToArray(),
                DefaultEmbed = new EmbedBuilder()
                    .WithDescription(
                        $"No top play found for user [{user.Username}](https://osu.ppy.sh/users/{user.Id}) on mode {ruleset.RulesetInfo.Name}")
                    .Build()
            };
        }
    }
}