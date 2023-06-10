using Pepper.Commons.Maimai.HtmlParsers;
using Pepper.Commons.Maimai.Structures.Data;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        public async Task<SimpleUser?> GetFriendCode(string friendCode)
        {
            var html = await GetHtml(
                $"https://maimaidx-eng.com/maimai-mobile/friend/search/searchUser/?friendCode={friendCode}");
            if (html.Contains("WRONG CODE", StringComparison.Ordinal))
            {
                return null;
            }

            return SimpleUserParser.Parse(html);
        }
    }
}