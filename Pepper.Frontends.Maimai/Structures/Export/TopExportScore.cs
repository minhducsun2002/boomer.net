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
        public int Level { get; set; }

        [JsonProperty("level_decimal")]
        [JsonPropertyName("level_decimal")]
        public int LevelDecimal { get; set; }

        [JsonProperty("true_decimal")]
        [JsonPropertyName("true_decimal")]
        public bool TrueDecimal { get; set; }

        [JsonProperty("score")]
        [JsonPropertyName("score")]
        public int DxScore { get; set; }

        [JsonProperty("max_score")]
        [JsonPropertyName("max_score")]
        public int MaxDxScore { get; set; }

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

        [JsonProperty("genre_id")]
        [JsonPropertyName("genre_id")]
        public int GenreId { get; set; }

        [JsonProperty("genre")]
        [JsonPropertyName("genre")]
        public string? Genre { get; set; }
    }
}