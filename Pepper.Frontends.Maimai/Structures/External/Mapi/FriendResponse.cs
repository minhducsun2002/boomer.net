using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Maimai.Structures.External.Mapi
{
    public class FriendResponse
    {
        [JsonProperty("data")]
        [JsonPropertyName("data")]
        public Friend? Friend { get; set; }

        [JsonProperty("err")]
        [JsonPropertyName("err")]
        public string? Error { get; set; }

        [JsonProperty("success")]
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}