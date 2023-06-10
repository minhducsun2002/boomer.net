using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Maimai.Structures.Configuration
{
    public class GlobalConfiguration : Commons.Structures.Configuration.GlobalConfiguration
    {
        [JsonProperty("maimai")]
        [JsonPropertyName("maimai")]
        public MaimaiConfiguration? Maimai { get; set; }

        [JsonProperty("database")]
        [JsonPropertyName("database")]
        public DatabaseConfiguration? Database { get; set; }
    }
}