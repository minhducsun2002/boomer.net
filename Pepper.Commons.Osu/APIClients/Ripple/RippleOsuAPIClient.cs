using System.Net.Http;
using OsuSharp;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    [GameServer(GameServer.Ripple)]
    public partial class RippleOsuAPIClient : APIClient
    {
        private readonly OsuRestClient restClient;
        private readonly OsuClient legacyClient;

        /// <summary>
        /// Create an <see cref="APIClient"/> for Ripple server.
        /// </summary>
        /// <param name="httpClient">Self-explanatory.</param>
        /// <param name="restClient">OAuth2 REST client implementation.</param>
        public RippleOsuAPIClient(HttpClient httpClient, OsuRestClient restClient) : base(httpClient)
        {
            this.restClient = restClient;
            legacyClient = new OsuClient(new OsuSharpConfiguration
            {
                HttpClient = httpClient,
                ApiKey = "1",
                BaseUrl = "https://ripple.moe/api/"
            });
        }
    }
}