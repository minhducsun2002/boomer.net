using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Pepper.Frontends.Maimai.Utils;

namespace Pepper.Frontends.Maimai.Structures.Export
{
    public class TopExport
    {
        [JsonProperty("version")]
        [JsonPropertyName("version")]
        public int FormatVersion { get; } = 1;

        [JsonProperty("user")]
        [JsonPropertyName("user")]
        public TopExportUser? User { get; set; }

        [JsonProperty("maimai_version")]
        [JsonPropertyName("maimai_version")]
        public int MaimaiVersion { get; set; }

        [JsonProperty("timestamp")]
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; }

        [JsonProperty("coeff")]
        [JsonPropertyName("coeff")]
        public int[][] Coefficients { get; set; } = Array.Empty<int[]>();

        [JsonProperty("scores")]
        [JsonPropertyName("scores")]
        public TopExportScore[] TopExportScores { get; set; } = Array.Empty<TopExportScore>();
    }
}