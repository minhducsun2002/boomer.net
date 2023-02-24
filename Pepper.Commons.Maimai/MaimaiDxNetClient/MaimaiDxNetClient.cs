using Pepper.Commons.Maimai.Structures.Exceptions;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        private readonly string? clal;
        private static readonly HttpClient AuthHttpClient = new(new SocketsHttpHandler
        {
            AllowAutoRedirect = false
        });
        private readonly HttpClient httpClient;

        /// <param name="clalCookie">clal cookie without the <c>clal=</c> prefix</param>
        // ReSharper disable once InvalidXmlDocComment
        public MaimaiDxNetClient(HttpClient httpClient, string? clalCookie)
        {
            clal = clalCookie;
            this.httpClient = httpClient;
        }

        public async Task<string> GetHtml(string url)
        {
            var uid = await GetAuthUserId();
            if (uid == null)
            {
                throw new LoginFailedException();
            }
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.TryAddWithoutValidation("Cookie", $"userId={uid}");
            var file = await httpClient.SendAsync(request);
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