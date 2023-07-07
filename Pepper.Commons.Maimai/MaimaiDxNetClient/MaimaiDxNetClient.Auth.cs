using System.Net;
using Pepper.Commons.Maimai.Structures.Exceptions;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        private const string AuthUrl = "https://lng-tgk-aime-gw.am-all.net/common_auth/login" +
                                       "?site_id=maimaidxex" +
                                       "&redirect_url=https://maimaidx-eng.com/maimai-mobile/&back_url=https://maimai.sega.com/";
        private bool authenticated;
        private async Task Authenticate()
        {
            if (authenticated)
            {
                return;
            }
            var maimaiSsidRedemptionUrl = await VerifyCookie();

            var req = new HttpRequestMessage(HttpMethod.Get, maimaiSsidRedemptionUrl);
            var res = await ExecuteRequest(req, true, false);
            if (res.StatusCode == HttpStatusCode.ServiceUnavailable)
            {
                throw new MaintenanceException();
            }
            
            if (!res.Headers.TryGetValues("Set-Cookie", out var setCookies))
            {
                return;
            }

            if (!setCookies.Any(cookie => cookie.StartsWith("userId")))
            {
                throw new LoginFailedException();
            }

            authenticated = true;
        }

        public async Task<string> VerifyCookie()
        {
            if (clal == null)
            {
                throw new MissingCookieException();
            }

            var amAllReq = new HttpRequestMessage(HttpMethod.Get, AuthUrl);
            amAllReq.Headers.Add("Cookie", $"clal={clal}");
            var amAllRes = await authHttpClient.SendAsync(amAllReq);
            if (amAllRes.StatusCode != HttpStatusCode.Redirect)
            {
                throw new InvalidCookieException($"Status code was {(int) amAllRes.StatusCode}");
            }

            return amAllRes.Headers.Location!.ToString();
        }
    }
}