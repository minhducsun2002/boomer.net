using System.Net.Http;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    [GameServer(GameServer.Ripple)]
    public partial class RippleOsuAPIClient : APIClient
    {
        private readonly OsuRestClient restClient;

        /// <summary>
        /// Create an <see cref="APIClient"/> for Ripple server.
        /// </summary>
        /// <param name="httpClient">Self-explanatory.</param>
        /// <param name="restClient">OAuth2 REST client implementation.</param>
        public RippleOsuAPIClient(HttpClient httpClient, OsuRestClient restClient) : base(httpClient)
        {
            this.restClient = restClient;
        }
    }
}