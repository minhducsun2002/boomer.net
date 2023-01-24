using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Commands;
using Pepper.Frontends.Maimai.Commands.Button;
using Qommon;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;

namespace Pepper.Frontends.Maimai.Structures
{
    public partial class RecentScorePagedView
    {
        public static RecentScorePagedView Create(IEnumerable<(RecentRecord?, (Difficulty, Song, bool)?)> records)
        {
            var recordsArray = records.ToArray();
            var hasFailedParsing = false;
            var recentFiltered = recordsArray.Where(r =>
                {
                    if (r.Item1 == null)
                    {
                        hasFailedParsing = true;
                    }

                    return r.Item1 != null;
                })
                .ToArray() as (RecentRecord, (Difficulty, Song, bool)?)[];

            var grouped = GroupRecords(recentFiltered);
            var serializedPages = SerializeRecords(grouped, hasFailedParsing);
            var serializedInteractionIds = SerializeInteractions(grouped);

            return new RecentScorePagedView(
                new ListPageProvider(serializedPages),
                serializedInteractionIds
            );
        }

        private static List<List<(RecentRecord, (Difficulty, Song, bool)?)>> GroupRecords(
            IList<(RecentRecord, (Difficulty, Song, bool)?)> records)
        {
            var chunks = new List<List<(RecentRecord, (Difficulty, Song, bool)?)>>();
            var current = new List<(RecentRecord, (Difficulty, Song, bool)?)>();
            var last = records[0];
            foreach (var entry in records)
            {
                var (record, _) = entry;
                var (lastRecord, _) = last;
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
            IEnumerable<IEnumerable<(RecentRecord, (Difficulty, Song, bool)?)>> chunks)
        {
            var res = chunks
                .Select(chunk =>
                {
                    var res = chunk.Select(r =>
                    {
                        var (record, meta) = r;
                        var song = meta?.Item2;
                        var diff = meta?.Item1;
                        var interactionId = Compare.CreateCommand(
                            song?.Id, record.Name, record.Version, record.Difficulty,
                            diff?.Level,
                            diff?.LevelDecimal != null ? diff.LevelDecimal >= 7 : null
                        );

                        return (interactionId, record.Track);
                    });

                    return res.ToList();
                });

            return res.ToList();
        }

        private static IEnumerable<Page> SerializeRecords(List<List<(RecentRecord, (Difficulty, Song, bool hasMultipleVersions)?)>> chunks,
            bool hasFailedParsing)
        {
            return chunks.Select(recordGroup =>
            {
                var embeds = recordGroup.Select(entry =>
                {
                    var (record, song) = entry;
                    return ScoreFormatter.FormatScore(
                        record,
                        song?.Item1, null, record.Track, record.ImageUrl,
                        hasMultipleVersions: song?.hasMultipleVersions
                    );
                });
                if (chunks.Count != 1)
                {
                    embeds = embeds.Append(new LocalEmbed());
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