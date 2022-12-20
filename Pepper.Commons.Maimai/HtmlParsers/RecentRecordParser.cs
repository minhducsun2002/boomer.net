using System.Web;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures;
using Pepper.Commons.Maimai.Structures.Enums;
using Pepper.Commons.Maimai.Structures.Score;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static partial class RecentRecordParser
    {
        public static IEnumerable<RecentRecord?> Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.QuerySelectorAll(".p_10.t_l.f_0.v_b");
            var data = nodes.Select(node =>
            {
                try
                {
                    var top = node.QuerySelector(".playlog_top_container");
                    var topText = top.QuerySelector(".sub_title.t_c.f_r.f_11").ChildNodes
                        .Where(n => n.Name == "span")
                        .Take(2)
                        .ToArray();
                    var topImage = top.QuerySelector(".playlog_diff.v_b");
                    var topImageSrc = topImage.GetAttributeValue("src", "");
                    var difficulty = ParseDifficulty(topImageSrc);
                    var trackNumText = topText[0].InnerText;
                    var trackNum = trackNumText[^2..];
                    var track = PlayerDataParser.FastIntParse(trackNum);
                    var time = topText[1].InnerText;
                    var parsedTime = ParseTime(time);

                    bool? isWinningMatching = null;
                    var matchingResultNode = top.QuerySelector(".playlog_vs_result.v_b");
                    var src = matchingResultNode?.GetAttributeValue("src", "");
                    if (src != null)
                    {
                        isWinningMatching = ParseWinningMatching(src);
                    }

                    var content = top.NextSiblingElement();
                    var accuracyText = content.QuerySelector(".playlog_achievement_txt.t_r").InnerText;
                    var name = HttpUtility.HtmlDecode(content.QuerySelector(".basic_block.m_5.p_5.p_l_10.f_13.break")
                        .InnerText);
                    var rankImageSrc = content.QuerySelector(".playlog_scorerank").GetAttributeValue("src", "");
                    var chartTypeSrc = content.QuerySelector(".playlog_music_kind_icon").GetAttributeValue("src", "");
                    var version = ParseVersion(chartTypeSrc);
                    var accuracy = ParseAccuracy(accuracyText);
                    var (rank, rankPlus) = ParseRank(rankImageSrc);
                    var fcImage = content.QuerySelector(".playlog_score_block").NextSiblingElement();
                    var fcImageSrc = fcImage.GetAttributeValue("src", "");
                    var fcStatus = ParseFcStatus(fcImageSrc);
                    var multiplayerRank = 0;
                    var multiplayerRankImage = content.QuerySelector(".playlog_matching_icon.f_r");
                    if (multiplayerRankImage != null)
                    {
                        var multiplayerRankImageSrc = multiplayerRankImage.GetAttributeValue("src", "");
                        multiplayerRank = ParseMultiplayerRank(multiplayerRankImageSrc);
                    }

                    var fsImage = fcImage.NextSiblingElement();
                    var fsImageSrc = fsImage.GetAttributeValue("src", "");
                    var fsStatus = ParseSyncStatus(fsImageSrc);

                    var jacketImageUrl = content.QuerySelector(".music_img").GetAttributeValue("src", "");

                    var challengeNode = content.QuerySelector(".p_r.m_t_5.f_l.f_0");
                    var challengeType = ChallengeType.None;
                    int health = 0, maxHealth = 0;
                    if (challengeNode != null)
                    {
                        var challengeImage = challengeNode.QuerySelector(".h_30.p_l_5");
                        var challengeImageSrc = challengeImage.GetAttributeValue("src", "");
                        challengeType = ParseChallengeType(challengeImageSrc);
                        var challengeHealthNode = challengeNode.QuerySelector(".playlog_life_block");
                        var challengeHealthText = challengeHealthNode.InnerText;
                        (health, maxHealth) = AllRecordParser.ParseSlashedVsMaxStats(challengeHealthText);
                    }

                    return new RecentRecord
                    {
                        Track = track,
                        Timestamp = parsedTime,
                        Accuracy = accuracy,
                        Name = name,
                        Rank = rank,
                        RankPlus = rankPlus,
                        Version = version,
                        FcStatus = fcStatus,
                        MultiplayerRank = multiplayerRank,
                        SyncStatus = fsStatus,
                        Difficulty = difficulty,
                        ImageUrl = jacketImageUrl,
                        ChallengeType = challengeType,
                        ChallengeMaxHealth = maxHealth,
                        ChallengeRemainingHealth = health,
                        IsWinningMatching = isWinningMatching
                    };
                }
                catch
                {
                    return null;
                }
            });
            return data;
        }
    }
}