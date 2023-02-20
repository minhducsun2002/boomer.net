using System.Net;
using Pepper.Commons.Maimai.Structures.Exceptions;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        private const string AuthUrl = "https://lng-tgk-aime-gw.am-all.net/common_auth/login" +
                                       "?site_id=maimaidxex" +
                                       "&redirect_url=https://maimaidx-eng.com/maimai-mobile/&back_url=https://maimai.sega.com/";
        private async Task<string?> GetAuthUserId()
        {
            var maimaiSsidRedemptionUrl = await VerifyCookie();

            var req = new HttpRequestMessage(HttpMethod.Get, maimaiSsidRedemptionUrl);
            var res = await AuthHttpClient.SendAsync(req);
            if (res.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new MaintenanceException();
            }
            if (!res.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                return null;
            }

            var uidCookie = setCookies.FirstOrDefault(cookie => cookie.StartsWith("userId"));

            var cookieValue = uidCookie?.Split(';')[0].Split("userId=")[1];
            return cookieValue;
        }

        public async Task<string> VerifyCookie()
        {
            if (clal == null)
            {
                throw new MissingCookieException();
            }

            var amAllReq = new HttpRequestMessage(HttpMethod.Get, AuthUrl);
            amAllReq.Headers.Add("Cookie", $"clal={clal}");
            var amAllRes = await AuthHttpClient.SendAsync(amAllReq);
            if (amAllRes.StatusCode != HttpStatusCode.Redirect)
            {
                throw new InvalidCookieException($"Status code was {amAllRes.StatusCode}");
            }

            return amAllRes.Headers.Location!.ToString();
        }
    }
}