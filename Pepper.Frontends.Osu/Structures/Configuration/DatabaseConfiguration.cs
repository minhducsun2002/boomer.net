using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Osu.Structures.Configuration
{
    public class DatabaseConfiguration
    {
        [JsonProperty("main")]
        [JsonPropertyName("main")]
        public string? Main { get; set; }
    }
}