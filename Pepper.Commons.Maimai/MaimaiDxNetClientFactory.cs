namespace Pepper.Commons.Maimai
{
    public class MaimaiDxNetClientFactory
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICookieConsistencyLocker? locker;
        public MaimaiDxNetClientFactory(IHttpClientFactory httpClientFactory, ICookieConsistencyLocker? locker = null)
        {
            this.httpClientFactory = httpClientFactory;
            this.locker = locker;
        }
        public MaimaiDxNetClient Create(string clalCookie)
        {
            var httpClient = httpClientFactory.CreateClient();
            return new MaimaiDxNetClient(httpClient, clalCookie, locker);
        }
    }
}