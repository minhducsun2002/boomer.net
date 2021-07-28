using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Pepper.Services.Osu;
using Pepper.Structures.External.Osu;
using Pepper.Utilities.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class Score : OsuScoreCommand
    {
        public Score(ApiService service) : base(service) {}

        [Command("sc")]
        [Description("View/list scores on a certain map")]
        public async Task<DiscordCommandResult> Exec(
            [Description("A score URL or a beatmap ID.")] string link,
            [Remainder] [Description("Username to check. Default to your username, if set. Ignored if a score link is passed.")] Username username = null!
        )
        {
            var scoreParsingResult = URLParser.CheckScoreUrl(link, out var scoreLink);
            if (!scoreParsingResult)
                throw new ArgumentException("A valid score link must be passed!");
            var (mode, id) = scoreLink;
            var sc = await APIService.GetScore(
                id,
                Rulesets
                    .First(rulesetCheck => string.Equals(rulesetCheck.ShortName, mode, StringComparison.InvariantCultureIgnoreCase))
                    .RulesetInfo
                );
            return await SingleScore(sc);
        }
    }
}