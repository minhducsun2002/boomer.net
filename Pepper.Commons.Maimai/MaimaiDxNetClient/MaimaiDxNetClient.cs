using Pepper.Commons.Maimai.Structures.Exceptions;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        private readonly string clal;
        private static readonly HttpClient AuthHttpClient = new(new SocketsHttpHandler
        {
            AllowAutoRedirect = false
        });
        private readonly HttpClient httpClient;
        private readonly ICookieConsistencyLocker? locker;

        /// <param name="httpClient">Self-explanatory</param>
        /// <param name="clalCookie">clal cookie without the <c>clal=</c> prefix</param>
        /// <param name="locker">Cookie locker to ensure a single cookie is used at only one place at all times.</param>
        internal MaimaiDxNetClient(HttpClient httpClient, string clalCookie, ICookieConsistencyLocker? locker = null)
        {
            clal = clalCookie;
            this.httpClient = httpClient;
            this.locker = locker;
        }

        public async Task<string> GetHtml(string url)
        {
            HttpResponseMessage file;
            try
            {
                locker?.Lock(clal);
                var uid = await GetAuthUserId();
                if (uid == null)
                {
                    throw new LoginFailedException();
                }

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.TryAddWithoutValidation("Cookie", $"userId={uid}");
                file = await httpClient.SendAsync(request);
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
    }
}