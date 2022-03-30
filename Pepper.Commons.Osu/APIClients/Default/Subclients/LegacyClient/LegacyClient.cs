using System.Net.Http;
using OsuSharp;

namespace Pepper.Commons.Osu.APIClients.Default.Subclients
{
    internal partial class LegacyClient
    {
        private readonly OsuClient osuClient;
        public LegacyClient(DefaultOsuAPIClient.LegacyAPIToken legacyAPIToken, HttpClient httpClient)
            => osuClient = new OsuClient(new OsuSharpConfiguration
            {
                ApiKey = legacyAPIToken.Token,
                HttpClient = httpClient
            });
    }
}