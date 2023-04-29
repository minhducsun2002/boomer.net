using System.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures.Data.Score;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static partial class AllRecordParser
    {
        private static SongScoreEntry ParseMetadata(HtmlNode node)
        {
            var difficultyImage = node.QuerySelector(".h_20.f_l");
            var difficultyImageSrc = difficultyImage.GetAttributeValue("src", "");
            var difficulty = RecentRecordParser.ParseDifficulty(difficultyImageSrc);
            var name = node.QuerySelector(".music_name_block.t_l.f_13.break");
            var decodedName = HttpUtility.HtmlDecode(name.InnerText);
            var levelNode = node.QuerySelector(".music_lv_block.f_r.t_c.f_14");
            var levelText = levelNode.InnerText;
            var level = ParseLevel(levelText);
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
            var chartVersion = RecentRecordParser.ParseVersion(chartVersionImageSrc);
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
            var statRecords = main.QuerySelectorAll(".w_450.m_15.f_0")
                .Select(node =>
                {
                    var accuracyNode = node.QuerySelector(".music_score_block.w_120.t_r.f_l.f_12");
                    if (accuracyNode == null)
                    {
                        return null;
                    }

                    var meta = ParseMetadata(node);
                    var accuracyText = accuracyNode.InnerText;
                    var accuracy = RecentRecordParser.ParseAccuracy(accuracyText);
                    var rankImage = node.QuerySelector("input").PreviousSiblingElement().PreviousSiblingElement();
                    var rankImageSrc = rankImage.GetAttributeValue("src", "");
                    var (rank, rankPlus) = ParseRank(rankImageSrc);

                    var noteNode = node.QuerySelector(".music_score_block.w_180.t_r.f_l.f_12");
                    var noteText = noteNode.InnerText.Trim();
                    var (note, maxNote) = ParseSlashedVsMaxStats(noteText);

                    var fcImage = rankImage.PreviousSiblingElement();
                    var fcImageSrc = fcImage.GetAttributeValue("src", "");
                    var fcStatus = ParseFcStatus(fcImageSrc);
                    var fsImage = fcImage.PreviousSiblingElement();
                    var fsImageSrc = fsImage.GetAttributeValue("src", "");
                    var fsStatus = ParseSyncStatus(fsImageSrc);

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
                        MusicDetailLink = meta.MusicDetailLink
                    };
                })
                .Where(rec => rec != null);

            return statRecords as IEnumerable<TopRecord>;
        }
    }
}