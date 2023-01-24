using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Frontends.Maimai.Structures.Export
{
    public class TopExportScore
    {
        [JsonProperty("song")]
        [JsonPropertyName("song")]
        public string Song { get; set; }

        [JsonProperty("maimai_version")]
        [JsonPropertyName("maimai_version")]
        public int MaimaiVersion { get; set; }

        [JsonProperty("version")]
        [JsonPropertyName("version")]
        public ChartVersion ChartVersion { get; set; }

        [JsonProperty("level")]
        [JsonPropertyName("level")]
        public (int, int, bool) Level { get; set; } // level, decimal, true decimal or not

        [JsonProperty("score")]
        [JsonPropertyName("score")]
        public (int, int) DXScore { get; set; }

        [JsonProperty("accuracy")]
        [JsonPropertyName("accuracy")]
        public int Accuracy { get; set; }

        [JsonProperty("fc")]
        [JsonPropertyName("fc")]
        public FcStatus FcStatus { get; set; }

        [JsonProperty("sync")]
        [JsonPropertyName("sync")]
        public SyncStatus SyncStatus { get; set; }

        [JsonProperty("difficulty")]
        [JsonPropertyName("difficulty")]
        public Difficulty Difficulty { get; set; }
    }
}