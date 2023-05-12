using System.Net;
using System.Web;
using Pepper.Commons.Maimai.HtmlParsers;
using Pepper.Commons.Maimai.Structures.Data;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        public async Task<User> GetUserPlayData()
        {
            var html = await GetHtml("https://maimaidx-eng.com/maimai-mobile/playerData/");
            return PlayerDataParser.Parse(html);
        }

        public async Task<long> GetUserFriendCode()
        {
            var html = await GetHtml("https://maimaidx-eng.com/maimai-mobile/friend/userFriendCode/");
            return SelfFriendCodeParser.Parse(html);
        }

        public async Task<bool> Rename(string s)
        {
            var html = await GetHtml("https://maimaidx-eng.com/maimai-mobile/home/userOption/updateUserName/");
            var token = FriendPageParser.ParseToken(html);
            var r = new HttpRequestMessage(HttpMethod.Post,
                "https://maimaidx-eng.com/maimai-mobile/home/userOption/updateUserName/update/")
            {
                Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new("userName", HttpUtility.UrlEncode(s)),
                    new("token", token)
                })
            };

            var response = await AuthHttpClient.SendAsync(r);
            return response.StatusCode == HttpStatusCode.Redirect;
        }
    }
}