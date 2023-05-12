using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    public static class FriendPageParser
    {
        public static string ParseToken(string html)
        {
            var d = new HtmlDocument();
            d.LoadHtml(html);
            var root = d.DocumentNode;
            var token = root.QuerySelector("input[name=\"token\"]");
            return token.GetAttributeValue("value", "");
        }
    }
}