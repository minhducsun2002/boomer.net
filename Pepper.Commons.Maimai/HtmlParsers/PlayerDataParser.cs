using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    internal static partial class PlayerDataParser
    {
        public static User Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var main = doc.DocumentNode;

            var name = main.QuerySelector(".name_block");
            var avatar = main.QuerySelector(".w_112.f_l");
            var playCountText = main.QuerySelector(".m_5.m_t_10.t_r.f_12");
            var playCount = int.Parse(GetIntFromString(playCountText.InnerText));
            var danImageLink = main.QuerySelector(".h_35.f_l").GetAttributeValue("src", "");
            var danLevel = GetTwoDigitsData("rank_", danImageLink)!.Value;
            var seasonClassImageLink = main.QuerySelector(".p_l_10.h_35.f_l").GetAttributeValue("src", "");
            var seasonClass = GetTwoDigitsData("rank_s_", seasonClassImageLink)!.Value;
            var ratingNode = main.QuerySelector(".rating_block");
            var rating = int.Parse(ratingNode.InnerText);
            var starNode = main.QuerySelector(".p_l_10.f_l.f_14");
            var star = int.Parse(GetIntFromString(starNode.InnerText));

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
                Name = name.InnerText,
                Avatar = avatar.GetAttributeValue("src", ""),
                PlayCount = playCount,
                DanLevel = danLevel.Item1 * 10 + danLevel.Item2,
                Rating = rating,
                StarCount = star,
                SeasonClass = (SeasonClass) (seasonClass.Item1 * 10 + seasonClass.Item2),
                UserStatistics = stats
            };

            return user;
        }
    }
}