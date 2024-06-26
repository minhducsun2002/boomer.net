using Disqord;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Structures;

namespace Pepper.Frontends.Osu.Commands
{
    public partial class Scoreset : OsuScoreCommand
    {
        private const int MaxScorePerPage = 5;
        public Scoreset(APIClientStore s, BeatmapContextProviderService b, ModParserService p) : base(s, b, p) { }

        public static LocalEmbedField SerializeScoreInList(APIScore score, bool scoreLink = true)
        {
            var map = score.Beatmap!;
            var mapset = map.Metadata!;
            return new LocalEmbedField
            {
                Name = $@"{mapset.Artist} - {mapset.Title} [{map.DifficultyName}]"
                       + SerializeMods(score.Mods),
                Value = @$"[**{score.Rank}**] "
                        + (score.PP.HasValue
                            ? $"**{score.PP}**pp (**{score.Accuracy * 100:F3}**% | **{score.MaxCombo}**x)"
                            : $"**{score.Accuracy * 100:F3}**% - **{score.MaxCombo}**x")
                        + (score.Perfect ? " (FC)" : "")
                        + "\n"
                        + new BeatmapStatsSerializer(map).Serialize(formatted: true, serializationOptions: StatFilter.Statistics | StatFilter.BPM | StatFilter.StarRating)
                        + $"\n[{SerializeHitStats(score.Statistics, Rulesets[score.RulesetID].RulesetInfo)}]"
                        + $" @ {SerializeTimestamp(score.Date)}"
                        + $"\n[[**Beatmap**]](https://osu.ppy.sh/b/{map.OnlineID})"
                        + (scoreLink ? $" [[**Score**]](https://osu.ppy.sh/scores/{Rulesets[score.RulesetID].ShortName}/{score.OnlineID})" : "")
            };
        }

        private static LocalEmbed SerializeScoreset(IEnumerable<APIScore> scores, bool scoreLink = true)
            => new()
            {
                Fields = scores.Select(score => SerializeScoreInList(score, scoreLink)).ToList()
            };
    }
}