using System.Text.Json.Serialization;
using Disqord.Serialization.Json;

namespace Pepper.Frontends.Maimai.Structures.Export
{
    public class TopExportGenre
    {
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
    }
}