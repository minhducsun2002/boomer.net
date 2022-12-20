using System.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Score;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static partial class AllRecordParser
    {
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

                    var accuracyText = accuracyNode.InnerText;
                    var accuracy = RecentRecordParser.ParseAccuracy(accuracyText);
                    var name = node.QuerySelector(".music_name_block.t_l.f_13.break");
                    var decodedName = HttpUtility.HtmlDecode(name.InnerText);
                    var levelNode = node.QuerySelector(".music_lv_block.f_r.t_c.f_14");
                    var levelText = levelNode.InnerText;
                    var level = ParseLevel(levelText);
                    var difficultyImage = node.QuerySelector(".h_20.f_l");
                    var difficultyImageSrc = difficultyImage.GetAttributeValue("src", "");
                    var difficulty = RecentRecordParser.ParseDifficulty(difficultyImageSrc);
                    var rankImage = node.QuerySelector("input").PreviousSiblingElement().PreviousSiblingElement();
                    var rankImageSrc = rankImage.GetAttributeValue("src", "");
                    var (rank, rankPlus) = ParseRank(rankImageSrc);

                    var chartVersionImage = node.QuerySelector(".music_kind_icon");
                    chartVersionImage ??= node.QuerySelector(".music_kind_icon_dx");
                    chartVersionImage ??= node.QuerySelector(".music_kind_icon_standard");
                    var chartVersionImageSrc = chartVersionImage.GetAttributeValue("src", "");
                    var chartVersion = RecentRecordParser.ParseVersion(chartVersionImageSrc);
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
                        Name = decodedName,
                        Accuracy = accuracy,
                        Level = level,
                        Difficulty = difficulty,
                        Rank = rank,
                        RankPlus = rankPlus,
                        Version = chartVersion,
                        Notes = note,
                        MaxNotes = maxNote,
                        FcStatus = fcStatus,
                        SyncStatus = fsStatus
                    };
                })
                .Where(rec => rec != null);

            return statRecords as IEnumerable<TopRecord>;
        }
    }
}