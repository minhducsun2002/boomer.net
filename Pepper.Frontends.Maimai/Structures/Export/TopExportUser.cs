using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Maimai.Structures.Export
{
    public class TopExportUser
    {
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonProperty("rating")]
        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonProperty("dan")]
        [JsonPropertyName("dan")]
        public int DanLevel { get; set; }

        [JsonProperty("play_count")]
        [JsonPropertyName("play_count")]
        public int PlayCount { get; set; }
    }
}