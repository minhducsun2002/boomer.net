using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Commands.Button;
using Pepper.Frontends.Maimai.Utils;

namespace Pepper.Frontends.Maimai.Structures
{
    public class TopScorePageProvider : ScorePageProvider<TopRecord>
    {
        public const int ScorePerPage = 3;
        public readonly (Page, List<(string, int)>)[] OldPages;
        public readonly (Page, List<(string, int)>)[] NewPages;
        public int OldIndex => NewPages.Length;

        public TopScorePageProvider(IEnumerable<ScoreWithMeta<TopRecord>> newRecords, IEnumerable<ScoreWithMeta<TopRecord>> oldRecords)
            : base(ArraySegment<ScoreWithMeta<TopRecord>>.Empty)
        {
            var @new = newRecords.ToList();
            var old = oldRecords.ToList();

            var concat = new[] { @new, old };
            var groupedPages = concat.Select((list, index) =>
            {
                var isNew = index == 0;
                var scores = list
                    .Where(p => p.Rating != null)
                    .OrderByDescending(p => p.Rating)
                    .ThenByDescending(p => p.ChartConstant)
                    .ThenByDescending(p => p.Score.Accuracy)
                    .ToList();
                var chunked = scores.Chunk(ScorePerPage);
                var sum = Calculate.NormalizedRating(list.Sum(d => d.Rating)!.Value);
                var footer = new LocalEmbedFooter().WithText($"Total {(isNew ? "New" : "Old")} rating : {sum}, avg {sum / list.Count}");
                var parsed = chunked.Select((chunk, pageIndex) =>
                    {
                        var embeds = chunk.Select(
                                (sc, index1) =>
                                    ScoreFormatter<TopRecord>.FormatScore(sc, pageIndex * ScorePerPage + index1 + 1)
                            )
                            .Append(new LocalEmbed().WithFooter(footer));
                        var scoreCheckInteractionIds = chunk
                            .Select((sc, index1) =>
                            {
                                var command = Compare.CreateCommand(
                                    sc.Song?.Id, sc.Score.Name, sc.Score.Version,
                                    sc.Score.Difficulty, sc.Difficulty?.Level, sc.Difficulty?.LevelDecimal == 7
                                );
                                var index = pageIndex * ScorePerPage + index1 + 1;
                                return (command, index);
                            })
                            .ToList();
                        var page = new Page().WithEmbeds(embeds).WithContent("These calculations are estimated.");
                        return (page, interactions: scoreCheckInteractionIds);
                    })
                    .ToList();
                return parsed;
            })
                .ToArray();

            NewPages = groupedPages[0].ToArray();
            OldPages = groupedPages[1].ToArray();
        }

        public (Page, List<(string, int)>)? GetPage(int index)
        {
            var res = index >= OldIndex
                ? OldPages.ElementAtOrDefault(index - OldIndex)
                : NewPages.ElementAtOrDefault(index);

            return res == default ? null : res;
        }

        public override ValueTask<Page?> GetPageAsync(PagedViewBase view)
        {
            var index = view.CurrentPageIndex;
            var p = GetPage(index);
            return new ValueTask<Page?>(p?.Item1);
        }

        public override int PageCount => OldPages.Length + NewPages.Length;
    }
}