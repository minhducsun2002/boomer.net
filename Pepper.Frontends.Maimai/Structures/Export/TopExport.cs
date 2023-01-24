using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Pepper.Frontends.Maimai.Utils;

namespace Pepper.Frontends.Maimai.Structures.Export
{
    public class TopExport
    {
        [JsonProperty("version")]
        [JsonPropertyName("version")]
        public readonly int FormatVersion = 1;

        [JsonProperty("maimai_version")]
        [JsonPropertyName("maimai_version")]
        public int MaimaiVersion { get; set; }

        [JsonProperty("timestamp")]
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("coeff")]
        [JsonPropertyName("coeff")]
        public (int, int)[] Coefficients = Calculate.Coeff;

        [JsonProperty("scores")]
        [JsonPropertyName("scores")]
        public TopExportScore[] TopExportScores = Array.Empty<TopExportScore>();
    }
}