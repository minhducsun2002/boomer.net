using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Osu.Structures.Configuration
{
    public class OsuApiConfiguration
    {
        [JsonProperty("v1")]
        [JsonPropertyName("v1")]
        public string? V1ApiKey { get; set; }

        [JsonProperty("v2")]
        [JsonPropertyName("v2")]
        public OsuApiV2Configuration? V2 { get; set; }
    }

    public class OsuApiV2Configuration
    {
        [JsonProperty("client_secret")]
        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; set; }

        [JsonProperty("client_id")]
        [JsonPropertyName("client_id")]
        public int ClientId { get; set; }
    }
}