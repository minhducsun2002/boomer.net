using System.Collections.Generic;
using System.Linq;
using Disqord;
using Pepper.Services.Osu.API;

namespace Pepper.Commands.Osu
{
    public partial class Scoreset : OsuScoreCommand
    {
        private const int MaxScorePerPage = 5;
        
        private static LocalEmbed SerializeScoreset(IEnumerable<APILegacyScoreInfo> scores)
            => new()
            {
                Fields = scores.Select(score =>
                {
                    var map = score.Beatmap!;
                    var mapset = map.Metadata!;
                    return new LocalEmbedField
                    {
                        Name = $@"{mapset.Artist} - {mapset.Title} [{map.Version}]"
                               + (score.Mods.Any() ? "+" + string.Join("", score.Mods) : ""),
                        Value = @$"[**{score.Rank}**] "
                                + (score.PP.HasValue
                                    ? $"**{score.PP}**pp (**{score.Accuracy * 100:F3}**% | **{score.MaxCombo}**x)"
                                    : $"**{score.Accuracy * 100:F3}**% - **{score.MaxCombo}**x")
                                + (score.Perfect ? " (FC)" : "")
                                + $"\n{SerializeBeatmapStats(map, showLength: false, delimiter: '-')}"
                                + $"\n[{SerializeHitStats(score.Statistics)}] @ **{SerializeTimestamp(score.Date, false)}** `UTC`"
                                + $"\n[[**Beatmap**]](https://osu.ppy.sh/b/{map.OnlineBeatmapID})"
                                + $" [[**Score**]](https://osu.ppy.sh/scores/{Rulesets[score.OnlineRulesetID].ShortName}/{score.OnlineScoreID})"
                    };
                }).ToList()
            };
    }
}