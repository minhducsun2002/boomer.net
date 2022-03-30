using System.Net.Http;
using Pepper.Commons.Osu.APIClients.Default.Subclients;

namespace Pepper.Commons.Osu.APIClients.Default
{
    [GameServer(GameServer.Osu)]
    public partial class DefaultOsuAPIClient : APIClient
    {
        private readonly OsuRestClient restClient;
        private readonly ScrapingClient scrapingClient;
        private readonly LegacyClient? legacyClient;
        public DefaultOsuAPIClient(HttpClient httpClient, OsuRestClient restClient, LegacyAPIToken? legacyAPIToken = null) : base(httpClient)
        {
            this.restClient = restClient;
            scrapingClient = new ScrapingClient(httpClient);
            if (legacyAPIToken != null)
            {
                legacyClient = new LegacyClient(legacyAPIToken, httpClient);
            }
        }
    }
}