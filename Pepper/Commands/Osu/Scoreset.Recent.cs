using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Discord;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Structures.Commands;
using Pepper.Structures.Commands.Result;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public partial class Scoreset
    {
        [Command("recent", "recentplay", "rp")]
        [Description("Show recent plays of a player.")]
        public async Task<EmbedResult> Recent(
            [Flag("/")] [Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            [Description("Username to check. Default to your username, if set.")] Username username
        )
        {
            var rulesetInfo = ruleset.RulesetInfo;
            var (user, _, _) = await ApiService.GetUser(username, rulesetInfo);

            var scores = await ApiService.GetUserScores(user.Id, ScoreType.Recent, rulesetInfo);
            var chunks = scores.Chunk(MaxScorePerPage).ToArray();
            return new EmbedResult
            {
                Embeds = chunks.Select((embed, index) => SerializeScoreset(embed)
                        .WithFooter($"Recent plays - Page {index + 1}/{chunks.Length}")
                        .WithAuthor(SerializeAuthorBuilder(user))
                        .Build())
                    .ToArray(),
                DefaultEmbed = new EmbedBuilder()
                    .WithDescription(
                        $@"No recent play found for user [{user.Username}](https://osu.ppy.sh/users/{user.Id}) on mode {rulesetInfo.Name}"
                        )
                    .Build()
            };
        }

        [Command("rs")]
        [Description("Show the most recent play of a player.")]
        public async Task<EmbedResult> MostRecent(
            [Flag("/")] [Description("Game mode to check. Defaults to osu!.")] Ruleset ruleset,
            [Description("Username to check. Default to your username, if set.")] Username username,
            [Flag("#")] [Description("Index from the latest play. 1 indicates the latest.")] int pos = 1)
        {
            var rulesetInfo = ruleset.RulesetInfo;
            var (user, _, _) = await ApiService.GetUser(username, rulesetInfo);
            var scores = await ApiService.GetUserScores(user.Id, ScoreType.Recent, rulesetInfo, 1, pos - 1);
            
            if (scores.Any()) return await SingleScore(scores.First());
            return new EmbedResult
            {
                DefaultEmbed = new EmbedBuilder()
                    .WithDescription(
                        $"No recent play found for user [{user.Username}](https://osu.ppy.sh/users/{user.Id}) on mode {rulesetInfo.Name} at position #{pos}."
                    )
                    .Build()
            };
        }
    }
}