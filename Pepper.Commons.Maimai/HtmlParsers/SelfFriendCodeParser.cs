using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace Pepper.Commons.Maimai.HtmlParsers
{
    internal static class SelfFriendCodeParser
    {
        public static long Parse(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var node = doc.DocumentNode;
            var idNode = node.QuerySelector(".see_through_block.t_c");
            return long.Parse(idNode.InnerText);
        }
    }
}