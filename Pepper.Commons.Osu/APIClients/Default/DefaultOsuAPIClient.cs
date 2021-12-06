using System.Net.Http;
using OsuSharp;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient : APIClient
    {
        private readonly OsuClient legacyApiClient;
        public DefaultOsuAPIClient(HttpClient httpClient, OsuClient legacyApiClient) : base(httpClient)
        {
            this.legacyApiClient = legacyApiClient;
        }

        public DefaultOsuAPIClient(HttpClient httpClient, string apiv1Key) : base(httpClient)
        {
            legacyApiClient = new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = apiv1Key
            });
        }
    }
}