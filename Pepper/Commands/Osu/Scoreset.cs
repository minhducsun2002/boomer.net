using System.Collections.Generic;
using System.Linq;
using Disqord;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu;

namespace Pepper.Commands.Osu
{
    public partial class Scoreset : OsuScoreCommand
    {
        private const int MaxScorePerPage = 5;
        public Scoreset(APIClientStore s, BeatmapContextProviderService b, ModParserService p) : base(s, b, p) { }

        public static LocalEmbedField SerializeScoreInList(APIScore score, bool utcHint = false, bool scoreLink = true)
        {
            var map = score.Beatmap!;
            var mapset = map.Metadata!;
            return new LocalEmbedField
            {
                Name = $@"{mapset.Artist} - {mapset.Title} [{map.DifficultyName}]"
                       + (score.Mods.Any() ? "+" + string.Join("", score.Mods) : ""),
                Value = @$"[**{score.Rank}**] "
                        + (score.PP.HasValue
                            ? $"**{score.PP}**pp (**{score.Accuracy * 100:F3}**% | **{score.MaxCombo}**x)"
                            : $"**{score.Accuracy * 100:F3}**% - **{score.MaxCombo}**x")
                        + (score.Perfect ? " (FC)" : "")
                        + $"\n{SerializeBeatmapStats(map, showLength: false, delimiter: '-')}"
                        + $"\n[{SerializeHitStats(score.Statistics, Rulesets[score.RulesetID].RulesetInfo)}] @ **{SerializeTimestamp(score.Date, false)}**{(utcHint ? " `UTC`" : "")}"
                        + $"\n[[**Beatmap**]](https://osu.ppy.sh/b/{map.OnlineID})"
                        + (scoreLink ? $" [[**Score**]](https://osu.ppy.sh/scores/{Rulesets[score.RulesetID].ShortName}/{score.OnlineID})" : "")
            };
        }

        private static LocalEmbed SerializeScoreset(IEnumerable<APIScore> scores, bool utcHint = false, bool scoreLink = true)
            => new()
            {
                Fields = scores.Select(score => SerializeScoreInList(score, utcHint, scoreLink)).ToList()
            };
    }
}