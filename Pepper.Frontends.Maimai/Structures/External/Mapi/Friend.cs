using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Maimai.Structures.External.Mapi
{
    public class Friend
    {
#pragma warning disable CS8618
        [JsonProperty("name")]
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonProperty("rating")]
        [JsonPropertyName("rating")]
        public int Rating { get; set; }

        [JsonProperty("dan")]
        [JsonPropertyName("dan")]
        public int Dan { get; set; }

        [JsonProperty("class")]
        [JsonPropertyName("class")]
        public int Class { get; set; }

        [JsonProperty("avatar")]
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
#pragma warning restore CS8618
    }
}