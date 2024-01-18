using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    internal static partial class PlayerDataParser
    {
        public static User Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var main = doc.DocumentNode;

            var simple = SimpleUserParser.Parse(main);
            var playCountText = main.QuerySelector(".m_5.m_b_5.t_r.f_12");
            var playCount = NumericParsingUtils.FastIntParseIgnoreCommaAndSpace(GetIntFromString(playCountText.ChildNodes[2].InnerText));

            var statRecords = main.QuerySelectorAll(".musiccount_img_block > img")
                .Select(img =>
                {
                    const string initial = "music_icon_", ending = ".png";

                    var url = img.GetAttributeValue("src", "");
                    var initialPosition = url.IndexOf(initial, StringComparison.Ordinal);
                    var skimmedIconUrl = url.Substring(
                        initialPosition + initial.Length,
                        url.IndexOf(ending, StringComparison.Ordinal) - (initialPosition + initial.Length)
                    );
                    var content = img.ParentNode.NextSiblingElement().InnerText;
                    return (skimmedIconUrl, content);
                })
                .ToDictionary(pair => pair.skimmedIconUrl, pair => pair.content);

            var stats = GetUserStatistics(statRecords);

            var user = new User
            {
                Name = simple.Name,
                Avatar = simple.Avatar,
                PlayCount = playCount,
                DanLevel = simple.DanLevel,
                Rating = simple.Rating,
                StarCount = simple.StarCount,
                SeasonClass = simple.SeasonClass,
                UserStatistics = stats
            };

            return user;
        }
    }
}