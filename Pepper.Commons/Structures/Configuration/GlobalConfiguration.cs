using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Pepper.Commons.Structures.Configuration
{
    public class GlobalConfiguration
    {
        [JsonProperty("logging")]
        [JsonPropertyName("logging")]
        public LoggingConfiguration? Logging { get; set; }
    }
}