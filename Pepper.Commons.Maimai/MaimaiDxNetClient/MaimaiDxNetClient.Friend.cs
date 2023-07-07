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

            return SimpleUserParser.Parse(html, out _);
        }

        public async Task<bool> AddFriend(string friendCode)
        {
            var url = $"https://maimaidx-eng.com/maimai-mobile/friend/search/searchUser/?friendCode={friendCode}";
            var html = await GetHtml(url);
            if (html.Contains("WRONG CODE", StringComparison.Ordinal))
            {
                return false;
            }
            
            SimpleUserParser.Parse(html, out var friendToken);
            if (friendToken == null)
            {
                return false;
            }

            var req = new HttpRequestMessage(HttpMethod.Post, 
                "https://maimaidx-eng.com/maimai-mobile/friend/search/invite/");
            req.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("idx", friendCode),
                new KeyValuePair<string, string>("token", friendToken),
                new KeyValuePair<string, string>("invite", "")
            });
            req.Headers.Add("Referer", url);
            var r = await req.Content.ReadAsStringAsync();

            var res = await ExecuteRequest(req, true);
            return res.Headers.Location?.ToString() == "https://maimaidx-eng.com/maimai-mobile/friend/invite/";
        }
    }
}