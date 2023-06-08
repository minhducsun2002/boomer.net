using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Osu.Structures.Configuration
{
    public class GlobalConfiguration : Commons.Structures.Configuration.GlobalConfiguration
    {
        [JsonProperty("osu")]
        [JsonPropertyName("osu")]
        public OsuApiConfiguration? Osu { get; set; }

        [JsonProperty("database")]
        [JsonPropertyName("database")]
        public DatabaseConfiguration? Database { get; set; }
    }
}