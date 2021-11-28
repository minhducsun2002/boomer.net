using System.Net.Http;
using OsuSharp;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient : IAPIClient
    {
        private readonly HttpClient httpClient;
        private readonly OsuClient legacyApiClient;
        public DefaultOsuAPIClient(HttpClient httpClient, OsuClient legacyApiClient)
        {
            this.httpClient = httpClient;
            this.legacyApiClient = legacyApiClient;
        }

        public DefaultOsuAPIClient(HttpClient httpClient, string apiv1Key)
        {
            this.httpClient = httpClient;
            legacyApiClient = new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = apiv1Key
            });
        }
    }
}