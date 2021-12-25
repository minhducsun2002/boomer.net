using System.Net.Http;
using OsuSharp;

namespace Pepper.Commons.Osu.APIClients.Ripple
{
    [GameServer(GameServer.Ripple)]
    public partial class RippleOsuAPIClient : APIClient
    {
        private readonly OsuClient legacyApiClient;
        public RippleOsuAPIClient(HttpClient httpClient) : base(httpClient)
        {
            legacyApiClient = new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = "placeholderApiKey",
                BaseUrl = "https://ripple.moe/api"
            });
        }
    }
}