using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Frontends.Maimai.Structures.Configuration
{
    public class MaimaiConfiguration
    {
        [JsonProperty("dump_password")]
        [JsonPropertyName("dump_password")]
        public string DumpPassword { get; set; } = "";

        [JsonProperty("friend_api_server")]
        [JsonPropertyName("friend_api_server")]
        public string? FriendApiServer { get; set; }
    }
}