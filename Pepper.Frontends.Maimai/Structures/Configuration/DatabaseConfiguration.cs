using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Maimai.Structures.Configuration
{
    public class DatabaseConfiguration
    {
        [JsonProperty("main")]
        [JsonPropertyName("main")]
        public string? Main { get; set; }

        [JsonProperty("maimai")]
        [JsonPropertyName("maimai")]
        public string? Maimai { get; set; }
    }
}