using System.Net;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        private const string AuthUrl = "https://lng-tgk-aime-gw.am-all.net/common_auth/login" +
                                       "?site_id=maimaidxex" +
                                       "&redirect_url=https://maimaidx-eng.com/maimai-mobile/&back_url=https://maimai.sega.com/";
        public async Task<string?> GetAuthUserId()
        {
            var maimaiSsidRedemptionUrl = await VerifyCookie();
            if (maimaiSsidRedemptionUrl == null)
            {
                return null;
            }

            var req = new HttpRequestMessage(HttpMethod.Get, maimaiSsidRedemptionUrl);
            var res = await AuthHttpClient.SendAsync(req);
            if (!res.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                return null;
            }

            var uidCookie = setCookies.FirstOrDefault(cookie => cookie.StartsWith("userId"));

            var cookieValue = uidCookie?.Split(';')[0].Split("userId=")[1];
            return cookieValue;
        }

        private async Task<string?> VerifyCookie()
        {
            var amAllReq = new HttpRequestMessage(HttpMethod.Get, AuthUrl);
            amAllReq.Headers.Add("Cookie", $"clal={clal}");
            var amAllRes = await AuthHttpClient.SendAsync(amAllReq);
            return amAllRes.StatusCode != HttpStatusCode.Redirect
                ? null
                // we're fucked
                : amAllRes.Headers.Location!.ToString();
        }
    }
}