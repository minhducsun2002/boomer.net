using System.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures.Data.Score;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static class ChartRecordParser
    {
        public static IEnumerable<ChartRecord> Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var main = doc.DocumentNode;
            var name = main.QuerySelector(".m_5.f_15.break");
            var decodedName = HttpUtility.HtmlDecode(name.InnerText);
            var scoreNodes = doc.QuerySelectorAll(".w_450.m_15.f_0")
                .Select(node =>
                {
                    var accuracyNode = node.QuerySelector(".music_score_block.w_120.d_ib.t_r.f_12");
                    var accuracyText = accuracyNode.InnerText;
                    var accuracy = RecentRecordParser.ParseAccuracy(accuracyText);
                    var levelNode = node.QuerySelector(".music_lv_back");
                    var levelText = levelNode.InnerText;
                    var level = AllRecordParser.ParseLevel(levelText);
                    var difficultyImage = node.QuerySelector(".h_20.f_l");
                    var difficultyImageSrc = difficultyImage.GetAttributeValue("src", "");
                    var difficulty = RecentRecordParser.ParseDifficulty(difficultyImageSrc);
                    var rankImage = node.QuerySelector(".p_t_5.v_t");
                    var rankImageSrc = rankImage.GetAttributeValue("src", "");
                    var (rank, rankPlus) = AllRecordParser.ParseRank(rankImageSrc);

                    var chartVersionImage = node.QuerySelector(".music_kind_icon");
                    var chartVersionImageSrc = chartVersionImage.GetAttributeValue("src", "");
                    var chartVersion = RecentRecordParser.ParseVersion(chartVersionImageSrc);

                    var playStatsTableNode = node.QuerySelector("table"); // table -> tr, no tbody here
                    // apparently it's #text -> tr -> #text -> tr -> #text
                    var lastPlayedNode = playStatsTableNode.ChildNodes[1].ChildNodes[3]; // the row is ["Last played date: ", (last played time)]
                    var playCountNode = playStatsTableNode.ChildNodes[3].ChildNodes[3]; // the row is ["PLAY COUNT: ", (play count)]
                    var lastPlayedText = lastPlayedNode.InnerText;
                    var playCountText = playCountNode.InnerText;
                    var lastPlayed = RecentRecordParser.ParseTime(lastPlayedText);
                    var playCount = PlayerDataParser.FastIntParseIgnoreCommaAndSpace(playCountText);

                    var noteNode = node.QuerySelector(".music_score_block.w_310.m_r_0.d_ib.t_r.f_12");
                    var noteText = noteNode.InnerText.Trim();
                    var (note, maxNote) = AllRecordParser.ParseSlashedVsMaxStats(noteText);

                    var fcImage = rankImage.NextSiblingElement();
                    var fcImageSrc = fcImage.GetAttributeValue("src", "");
                    var fcStatus = AllRecordParser.ParseFcStatus(fcImageSrc);
                    var fsImage = fcImage.NextSiblingElement();
                    var fsImageSrc = fsImage.GetAttributeValue("src", "");
                    var fsStatus = AllRecordParser.ParseSyncStatus(fsImageSrc);

                    return new ChartRecord
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
                        SyncStatus = fsStatus,
                        MusicDetailLink = null,
                        LastPlayed = lastPlayed,
                        PlayCount = playCount
                    };
                });

            return scoreNodes;
        }
    }
}