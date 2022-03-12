using System.Net.Http;
using OsuSharp;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    [GameServer(GameServer.Ripple)]
    public partial class RippleOsuAPIClient : APIClient
    {
        private readonly OsuRestClient restClient;
        private readonly OsuClient osuSharpClient;

        /// <summary>
        /// Create an <see cref="APIClient"/> for Ripple server.
        /// </summary>
        /// <param name="httpClient">Self-explanatory.</param>
        /// <param name="restClient">OAuth2 REST client implementation.</param>
        /// <param name="osuSharpClient">Self-explanatory.</param>
        public RippleOsuAPIClient(HttpClient httpClient, OsuRestClient restClient, OsuClient osuSharpClient) : base(httpClient)
        {
            this.restClient = restClient;
            this.osuSharpClient = osuSharpClient;
        }
    }
}