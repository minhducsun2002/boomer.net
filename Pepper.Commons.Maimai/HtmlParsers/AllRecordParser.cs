using System.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures.Data.Score;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static class AllRecordParser
    {
        private static SongScoreEntry ParseMetadata(HtmlNode node)
        {
            var difficultyImage = node.QuerySelector(".h_20.f_l");
            var difficultyImageSrc = difficultyImage.GetAttributeValue("src", "");
            var difficulty = ImageLinkParsingUtils.ParseDifficulty((ReadOnlySpan<char>) difficultyImageSrc);
            var name = node.QuerySelector(".music_name_block.t_l.f_13.break");
            var decodedName = HttpUtility.HtmlDecode(name.InnerText);
            var levelNode = node.QuerySelector(".music_lv_block.f_r.t_c.f_14");
            var levelText = levelNode.InnerText;
            var level = NumericParsingUtils.ParseLevel((ReadOnlySpan<char>) levelText);
            var chartVersionImage = node.QuerySelector(".music_kind_icon");
            if (chartVersionImage == null)
            {
                var child = node.GetChildElements().ToArray();
                for (var i = 0; i < child.Length; i++)
                {
                    var e = child[i];
                    if (e.Name == "img" && e.GetAttributeValue("onclick", "123") == "123")
                    {
                        chartVersionImage = e;
                        break;
                    }
                }
            }

            var chartVersionImageSrc = chartVersionImage!.GetAttributeValue("src", "");
            var chartVersion = ImageLinkParsingUtils.ParseVersion((ReadOnlySpan<char>) chartVersionImageSrc);
            var detailLinkNode = node.QuerySelector("form > input");
            var detailLink = detailLinkNode?.GetAttributeValue("value", "");
            return new SongScoreEntry
            {
                Name = decodedName,
                Difficulty = difficulty,
                Level = level,
                Version = chartVersion,
                MusicDetailLink = detailLink
            };
        }

        public static IEnumerable<SongScoreEntry> ParseMeta(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var main = doc.DocumentNode;
            var statRecords = main.QuerySelectorAll(".w_450.m_15.f_0")
                .Select(ParseMetadata);
            return statRecords;
        }

        public static IEnumerable<TopRecord> Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var main = doc.DocumentNode;
            var elementList = EnumerateElements(main.QuerySelector(".screw_block").ParentNode);
            var statRecords = elementList
                .Select(pair =>
                {
                    var (genre, node) = pair;
                    var accuracyNode = node.QuerySelector(".music_score_block.w_120.t_r.f_l.f_12");
                    if (accuracyNode == null)
                    {
                        return null;
                    }

                    var meta = ParseMetadata(node);
                    var accuracyText = accuracyNode.InnerText;
                    var accuracy = NumericParsingUtils.ParseAccuracy((ReadOnlySpan<char>) accuracyText);
                    var rankImage = node.QuerySelector("input").PreviousSiblingElement().PreviousSiblingElement();
                    var rankImageSrc = rankImage.GetAttributeValue("src", "");
                    var (rank, rankPlus) = ImageLinkParsingUtils.ParseRank((ReadOnlySpan<char>) rankImageSrc);

                    var noteNode = node.QuerySelector(".music_score_block.w_180.t_r.f_l.f_12");
                    var noteText = noteNode.InnerText.Trim();
                    var (note, maxNote) = NumericParsingUtils.ParseSlashedVsMaxStats((ReadOnlySpan<char>) noteText);

                    var fcImage = rankImage.PreviousSiblingElement();
                    var fcImageSrc = fcImage.GetAttributeValue("src", "");
                    var fcStatus = ImageLinkParsingUtils.ParseFcStatus((ReadOnlySpan<char>) fcImageSrc);
                    var fsImage = fcImage.PreviousSiblingElement();
                    var fsImageSrc = fsImage.GetAttributeValue("src", "");
                    var fsStatus = ImageLinkParsingUtils.ParseSyncStatus((ReadOnlySpan<char>) fsImageSrc);

                    return new TopRecord
                    {
                        Name = meta.Name,
                        Accuracy = accuracy,
                        Level = meta.Level,
                        Difficulty = meta.Difficulty,
                        Rank = rank,
                        RankPlus = rankPlus,
                        Version = meta.Version,
                        Notes = note,
                        MaxNotes = maxNote,
                        FcStatus = fcStatus,
                        SyncStatus = fsStatus,
                        MusicDetailLink = meta.MusicDetailLink,
                        Genre = genre
                    };
                })
                .Where(rec => rec != null);

            return statRecords as IEnumerable<TopRecord>;
        }

        private static IEnumerable<(string?, HtmlNode)> EnumerateElements(HtmlNode parent)
        {
            var child = parent.ChildNodes
                .Where(element => element is not HtmlTextNode)
                .ToArray();

            string? genre = null;
            foreach (var c in child)
            {
                if (c.HasClass("screw_block"))
                {
                    genre = c.InnerText;
                    continue;
                }

                if (c.HasClass("w_450"))
                {
                    yield return (genre, c);
                }
            }
        }
    }
}