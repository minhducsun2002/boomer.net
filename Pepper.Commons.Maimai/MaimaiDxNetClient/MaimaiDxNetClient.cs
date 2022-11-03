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

        /// <param name="clalCookie">clal cookie without the <c>clal=</c> prefix</param>
        // ReSharper disable once InvalidXmlDocComment
        public MaimaiDxNetClient(HttpClient httpClient, string clalCookie)
        {
            clal = clalCookie;
            this.httpClient = httpClient;
        }
    }
}