using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Commons.Structures.Configuration
{
    public class LoggingConfiguration
    {
        [JsonProperty("discord")]
        [JsonPropertyName("discord")]
        public string Discord { get; set; } = "";
    }
}