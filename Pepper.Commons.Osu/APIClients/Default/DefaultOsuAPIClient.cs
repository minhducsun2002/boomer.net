using System.Net.Http;

namespace Pepper.Commons.Osu.APIClients.Default
{
    [GameServer(GameServer.Osu)]
    public partial class DefaultOsuAPIClient : APIClient
    {
        private readonly OsuRestClient restClient;
        public DefaultOsuAPIClient(HttpClient httpClient, OsuRestClient restClient) : base(httpClient)
        {
            this.restClient = restClient;
        }
    }
}