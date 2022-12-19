using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Enums;
using Pepper.Commons.Maimai.Structures.Score;
using Pepper.Frontends.Maimai.Commands;
using Pepper.Frontends.Maimai.Commands.Button;
using Qommon;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;

namespace Pepper.Frontends.Maimai.Structures
{
    public partial class RecentScorePagedView
    {
        public static RecentScorePagedView Create(IEnumerable<(RecentRecord?, (Difficulty, Song)?)> records)
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
                .ToArray() as (RecentRecord, (Difficulty, Song)?)[];

            var grouped = GroupRecords(recentFiltered);
            var serializedPages = SerializeRecords(grouped, hasFailedParsing);
            var serializedInteractionIds = SerializeInteractions(grouped);

            return new RecentScorePagedView(
                new ListPageProvider(serializedPages),
                serializedInteractionIds
            );
        }

        private static List<List<(RecentRecord, (Difficulty, Song)?)>> GroupRecords(
            IList<(RecentRecord, (Difficulty, Song)?)> records)
        {
            var chunks = new List<List<(RecentRecord, (Difficulty, Song)?)>>();
            var current = new List<(RecentRecord, (Difficulty, Song)?)>();
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
            IEnumerable<IEnumerable<(RecentRecord, (Difficulty, Song)?)>> chunks)
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

        private static IEnumerable<Page> SerializeRecords(List<List<(RecentRecord, (Difficulty, Song)?)>> chunks,
            bool hasFailedParsing)
        {
            return chunks.Select(recordGroup =>
            {
                var embeds = recordGroup.Select(entry =>
                {
                    var (record, song) = entry;
                    var diff = MaimaiCommand.DifficultyStrings[(int) record.Difficulty];

                    var levelText = song.HasValue
                        ? song.Value.Item1.Level + "." + song.Value.Item1.LevelDecimal
                        : "";
                    int rating = default;
                    if (song.HasValue)
                    {
                        var (d, _) = song.Value;
                        var level = d.Level * 10 + d.LevelDecimal;
                        rating = MaimaiCommand.NormalizeRating(MaimaiCommand.GetFinalScore(record.Accuracy, level));
                    }

                    var rankEndingInPlus = record.Rank.EndsWith("plus");
                    var comboText = MaimaiCommand.GetStatusString(record.FcStatus);
                    var syncText = MaimaiCommand.GetStatusString(record.SyncStatus);

                    var r = new LocalEmbed
                    {
                        Author = new LocalEmbedAuthor()
                            .WithName($"Track {record.Track} - {diff} {levelText}"),
                        Title = $"{record.Name}",
                        Description = $"**{record.Accuracy / 10000}**.**{record.Accuracy % 10000:0000}**%" +
                                      $" - **{(rankEndingInPlus ? record.Rank[..^4].ToUpperInvariant() : record.Rank.ToUpperInvariant())}**"
                                      + (rankEndingInPlus ? "+" : "")
                                      + (comboText == "" ? comboText : $" [**{comboText}**]")
                                      + (syncText == "" ? syncText : $" [**{syncText}**]"),
                        ThumbnailUrl = record.ImageUrl ?? Optional<string>.Empty,
                        Timestamp = record.Timestamp,
                        Color = MaimaiCommand.GetColor(record.Difficulty)
                    };

                    if (record.ChallengeType != ChallengeType.None)
                    {
                        var hp = record.ChallengeRemainingHealth;
                        var maxHp = record.ChallengeMaxHealth;
#pragma warning disable CS8509
                        var text = record.ChallengeType switch
#pragma warning restore CS8509
                        {
                            ChallengeType.PerfectChallenge => $"Perfect Challenge : {hp}/{maxHp}",
                            ChallengeType.Course => $"Course : {hp}/{maxHp}"
                        };
                        r.Footer = new LocalEmbedFooter
                        {
                            Text = text
                        };
                    }

                    if (rating != default)
                    {
                        if (r.Footer.HasValue)
                        {
                            r.Footer.Value.Text = $"{rating} rating • " + r.Footer.Value.Text;
                        }
                        else
                        {
                            r = r.WithFooter($"{rating} rating");
                        }
                    }

                    return r;
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