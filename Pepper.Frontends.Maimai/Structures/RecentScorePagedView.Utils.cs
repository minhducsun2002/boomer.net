using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Commands.Button;

namespace Pepper.Frontends.Maimai.Structures
{
    public partial class RecentScorePagedView
    {
        public static RecentScorePagedView Create(IEnumerable<ScoreWithMeta<RecentRecord>?> records)
        {
            var recordsArray = records.ToArray();
            var hasFailedParsing = false;
            var recentFiltered = recordsArray.Where(r =>
                {
                    hasFailedParsing = r is null || hasFailedParsing;
                    return r != null;
                })
                .ToArray();

            var grouped = GroupRecords(recentFiltered!);
            var serializedPages = SerializeRecords(grouped, hasFailedParsing);
            var serializedInteractionIds = SerializeInteractions(grouped);

            return new RecentScorePagedView(
                new ListPageProvider(serializedPages),
                serializedInteractionIds
            );
        }

        private static List<List<ScoreWithMeta<RecentRecord>>> GroupRecords(
            IList<ScoreWithMeta<RecentRecord>> records)
        {
            var chunks = new List<List<ScoreWithMeta<RecentRecord>>>();
            var current = new List<ScoreWithMeta<RecentRecord>>();
            var last = records[0];
            foreach (var entry in records)
            {
                var record = entry.Score;
                var lastRecord = last.Score;
                if (record.Track >= lastRecord.Track)
                {
                    if (current.Count != 0)
                    {
                        chunks.Add(current);
                        current = new();
                    }
                }

                current.Add(entry);
                last = entry;
            }

            if (current.Count != 0)
            {
                chunks.Add(current);
            }

            return chunks;
        }

        private static List<List<(string, int)>> SerializeInteractions(
            IEnumerable<IEnumerable<ScoreWithMeta<RecentRecord>>> chunks)
        {
            var res = chunks
                .Select(chunk =>
                {
                    var res = chunk.Select(r =>
                    {
                        var record = r.Score;
                        var diff = r.Difficulty;
                        var interactionId = Compare.CreateCommand(
                            r.Song?.Id, record.Name, record.Version, record.Difficulty,
                            diff?.Level,
                            r.IsConstantAccurate ? diff!.LevelDecimal >= 7 : null
                        );

                        return (interactionId, record.Track);
                    });

                    return res.ToList();
                });

            return res.ToList();
        }

        private static IEnumerable<Page> SerializeRecords(IReadOnlyCollection<List<ScoreWithMeta<RecentRecord>>> chunks,
            bool hasFailedParsing)
        {
            return chunks.Select(recordGroup =>
            {
                var embeds = recordGroup.Select(
                    entry => ScoreFormatter<RecentRecord>.FormatScore(entry, entry.Score.Track, false)
                );
                var records = recordGroup.Select(r => r.Score).ToList();
                var isCourseCredit = records.All(record => record.ChallengeType == ChallengeType.Course);
                if (chunks.Count != 1 || isCourseCredit)
                {
                    var footer = new LocalEmbed();
                    if (isCourseCredit)
                    {
                        var sum = records.Select(r => r.Accuracy).Sum();
                        footer = footer.WithFooter($"Total : {sum / 10000}.{sum % 10000:0000}%");
                    }
                    embeds = embeds.Append(footer);
                }

                var page = new Page().WithEmbeds(embeds);
                if (hasFailedParsing)
                {
                    page = page.WithContent("Failed to parse some scores : grouping & ordering might be inaccurate.");
                }

                return page;
            });
        }
    }
}