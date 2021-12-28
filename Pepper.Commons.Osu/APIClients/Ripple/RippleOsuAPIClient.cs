using System.Net.Http;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    [GameServer(GameServer.Ripple)]
    public partial class RippleOsuAPIClient : APIClient
    {
        private readonly OsuOAuth2Client osuOAuth2Client;

        /// <summary>
        /// Create an <see cref="APIClient"/> for Ripple server.
        /// </summary>
        /// <param name="httpClient">Self-explanatory.</param>
        /// <param name="oAuth2Credentials">OAuth2 credentials for the application on official osu! servers.</param>
        public RippleOsuAPIClient(HttpClient httpClient, OsuOAuth2Credentials oAuth2Credentials) : base(httpClient)
        {
            osuOAuth2Client = new OsuOAuth2Client(httpClient, oAuth2Credentials);
        }
    }
}