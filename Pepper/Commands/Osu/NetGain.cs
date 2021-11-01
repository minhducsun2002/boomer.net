using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu;
using Pepper.Services.Osu.API;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class NetGain : OsuCommand
    {
        public NetGain(APIService service) : base(service) { }

        private class ScoreComparer : IComparer<APILegacyScoreInfo>
        {
            public int Compare(APILegacyScoreInfo? x, APILegacyScoreInfo? y)
            {
                return (y?.PP ?? 0).CompareTo(x?.PP ?? 0);
            }

            public static ScoreComparer Instance = new();
        }

        [Command("whatif", "if", "netgain")]
        [Description("What if you set a score with a certain pp amount?")]
        public async Task<DiscordCommandResult> Exec(
            [Flag("/")][Description("Game mode to check. Default to osu!.")] Ruleset ruleset,
            [Description("The amount of pp to check.")] int ppToCheck,
            [Description("Username to check. Default to your username, if set.")] Username username = null!
        )
        {
            var (user, _, _) = await APIService.GetUser(username, ruleset.RulesetInfo);
            var scores = await APIService.GetUserScores(user.Id, ScoreType.Best, ruleset.RulesetInfo);
            var sortedScores = scores.OrderByDescending(score => score.PP!.Value).ToList();

            var pps = sortedScores.Select((score, index) => (score.PP!.Value, index)).ToList();
            var previous = pps.Aggregate(0d, (sum, pair) => sum + pair.Value * Math.Pow(0.95, pair.index));
            var next = pps.Append((ppToCheck, 0))
                .OrderByDescending(pair => pair.Value).Select((pair, index) => (pair.Value, index))
                .Aggregate(0d, (sum, pair) => sum + pair.Value * Math.Pow(0.95, pair.index));

            var bonus = (double) user.Statistics.PP!.Value - previous;

            var embed = new LocalEmbed
            {
                Author = SerializeAuthorBuilder(user),
                Title = $"Setting a new {ppToCheck}pp score would change by +{next - previous:0.##}pp to {next + bonus:0.##}pp",
                Description = ""
            };

            if (ppToCheck > sortedScores[0].PP!.Value)
            {
                embed.Description += "\nThis will be the new __**best**__ play. The best play currently is :";
                embed.Fields = new List<LocalEmbedField> { Scoreset.SerializeScoreInList(sortedScores[0]) };
            }
            else if (ppToCheck < sortedScores[^1].PP!.Value)
            {
                embed.Description += "\nThis play won't appear on the best performance list.";
            }
            else
            {
                var scoreToCompare = new APILegacyScoreInfo { PP = ppToCheck };
                var lowerIndex = Array.BinarySearch(scores, scoreToCompare, ScoreComparer.Instance);
                if (lowerIndex < 0)
                {
                    lowerIndex = ~lowerIndex;
                }

                embed.Description += $"\nThis will be the new #__**{lowerIndex}**__ play. Here's how it will change :";

                embed.Fields = new List<LocalEmbedField>
                {
                    Scoreset.SerializeScoreInList(sortedScores[lowerIndex - 1]),
                    new LocalEmbedField { Name = $"[Your new {ppToCheck}pp score]" }.WithBlankValue(),
                    Scoreset.SerializeScoreInList(sortedScores[lowerIndex])
                };
            }

            return Reply(embed);
        }
    }
}