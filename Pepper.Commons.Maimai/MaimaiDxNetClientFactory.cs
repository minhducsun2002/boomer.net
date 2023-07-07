namespace Pepper.Commons.Maimai
{
    public class MaimaiDxNetClientFactory
    {
        internal static string LoginFactoryName = "maimai_login_client";
        internal static string ClientFactoryName = "maimai_client";

        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICookieConsistencyLocker? locker;
        public MaimaiDxNetClientFactory(IHttpClientFactory httpClientFactory, ICookieConsistencyLocker? locker = null)
        {
            this.httpClientFactory = httpClientFactory;
            this.locker = locker;
        }
        public MaimaiDxNetClient Create(string clalCookie)
        {
            var httpClient = httpClientFactory.CreateClient(ClientFactoryName);
            var loginHttpClient = httpClientFactory.CreateClient(LoginFactoryName);
            return new MaimaiDxNetClient(httpClient, loginHttpClient, clalCookie, locker);
        }
    }
}