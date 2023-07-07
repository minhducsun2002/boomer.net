using System.Text;
using Pepper.Commons.Maimai.Structures.Exceptions;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        private readonly string clal;
        private readonly HttpClient authHttpClient;
        private readonly HttpClient httpClient;
        private readonly ICookieConsistencyLocker? locker;
        private readonly Dictionary<string, string> cookieJar = new();

        /// <param name="httpClient">Self-explanatory</param>
        /// <param name="loginClient">HTTP client without redirection following, used for login.</param>
        /// <param name="clalCookie">clal cookie without the <c>clal=</c> prefix</param>
        /// <param name="locker">Cookie locker to ensure a single cookie is used at only one place at all times.</param>
        internal MaimaiDxNetClient(HttpClient httpClient, HttpClient loginClient, string clalCookie, ICookieConsistencyLocker? locker = null)
        {
            clal = clalCookie;
            this.httpClient = httpClient;
            authHttpClient = loginClient;
            this.locker = locker;
        }

        public async Task<string> GetHtml(string url)
        {
            var file = await ExecuteRequest(new HttpRequestMessage(HttpMethod.Get, url));
            var content = await file.Content.ReadAsStringAsync();
            var errorCodeIndex = content.IndexOf("ERROR CODE：", StringComparison.Ordinal);
            if (errorCodeIndex != -1)
            {
                var startIndex = errorCodeIndex + "ERROR CODE：".Length;
                var endIndex = content.IndexOf("<", startIndex, StringComparison.Ordinal);
                int.TryParse(content[startIndex..endIndex], out var code);
                throw new LoginFailedException(code);
            }
            return content;
        }

        private async Task<HttpResponseMessage> ExecuteRequest(HttpRequestMessage request, bool disableRedirect = false, bool auth = true)
        {
            try
            {
                locker?.Lock(clal);
                if (auth)
                {
                    await Authenticate();
                }

                var s = new StringBuilder();
                foreach (var (key, value) in cookieJar)
                {
                    s.Append(key);
                    s.Append('=');
                    s.Append(value);
                    s.Append(';');
                }

                request.Headers.TryAddWithoutValidation("Cookie", s.ToString());
                var response = await (disableRedirect ? authHttpClient : httpClient).SendAsync(request);

                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    foreach (var c in cookies)
                    {
                        var p = c.Split(';', 2);
                        var kv = p[0].Split('=');
                        cookieJar[kv[0]] = kv[1];
                    }
                }

                return response;
            }
            catch
            {
                locker?.Unlock(clal);
                throw;
            }
            finally
            {
                locker?.Unlock(clal);
            }
        }
    }
}