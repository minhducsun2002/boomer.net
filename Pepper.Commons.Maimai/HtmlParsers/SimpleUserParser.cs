using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using Pepper.Commons.Maimai.Structures.Data;
using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static class SimpleUserParser
    {
        public static SimpleUser Parse(string s)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(s);
            return Parse(doc.DocumentNode);
        }

        public static SimpleUser Parse(HtmlNode main)
        {
            var name = main.QuerySelector(".name_block");
            var avatar = main.QuerySelector(".w_112.f_l");
            var danImageLink = main.QuerySelector(".h_35.f_l").GetAttributeValue("src", "");
            var danLevel = PlayerDataParser.GetTwoDigitsData("rank_", danImageLink)!.Value;
            var seasonClassImageLink = main.QuerySelector(".p_l_10.h_35.f_l").GetAttributeValue("src", "");
            var seasonClass = PlayerDataParser.GetTwoDigitsData("rank_s_", seasonClassImageLink)!.Value;
            var ratingNode = main.QuerySelector(".rating_block");
            var rating = NumericParsingUtils.FastIntParse(ratingNode.InnerText);
            var starNode = main.QuerySelector(".p_l_10.f_l.f_14");
            var star = NumericParsingUtils.FastIntParse(PlayerDataParser.GetIntFromString(starNode.InnerText));
            return new SimpleUser
            {
                Name = name.InnerText,
                Avatar = avatar.GetAttributeValue("src", ""),
                DanLevel = danLevel.Item1 * 10 + danLevel.Item2,
                Rating = rating,
                StarCount = star,
                SeasonClass = (SeasonClass) (seasonClass.Item1 * 10 + seasonClass.Item2),
            };
        }
    }
}