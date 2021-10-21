using Newtonsoft.Json;
using osu.Game.Beatmaps;

namespace Pepper.Services.Osu.API
{
    public class APIBeatmap : osu.Game.Online.API.Requests.Responses.APIBeatmap
    {
        [JsonProperty(@"playcount")] public int PlayCount { get; set; }
        [JsonProperty(@"passcount")] public int PassCount { get; set; }
        [JsonProperty(@"mode_int")] public int Ruleset { get; set; }

        [JsonProperty(@"difficulty_rating")] public double StarDifficulty { get; set; }

        [JsonProperty(@"drain")] public float DrainRate { get; set; }
        [JsonProperty(@"cs")] public float CircleSize { get; set; }
        [JsonProperty(@"ar")] public float ApproachRate { get; set; }
        [JsonProperty(@"accuracy")] public float OverallDifficulty { get; set; }

        [JsonProperty(@"total_length")] public int TotalLengthInSeconds { get; set; }
        [JsonProperty(@"count_circles")] public int CircleCount { get; set; }
        [JsonProperty(@"count_sliders")] public int SliderCount { get; set; }

        [JsonProperty(@"version")] public string Version { get; set; }
        // [JsonProperty(@"failtimes")] public BeatmapMetrics Metrics { get; set; }

        [JsonProperty(@"max_combo")] public int? MaxCombo { get; set; }
    }
}