using System;
using Newtonsoft.Json;
using osu.Game.Beatmaps.Legacy;
using OsuSharp;

namespace Pepper.Commons.Osu.API.Ripple
{
#pragma warning disable CS8618
    internal class Score
    {
        [JsonProperty("id")] public int Id { get; set; }
        [JsonProperty("beatmap_md5")] public string BeatmapMd5 { get; set; }

        [JsonProperty("score")] public int TotalScore { get; set; }

        [JsonProperty("max_combo")] public int MaxCombo { get; set; }
        [JsonProperty("full_combo")] public bool FullCombo { get; set; }

        [JsonProperty("mods")] public LegacyMods Mods { get; set; }

        [JsonProperty("count_300")] public int Count300 { get; set; }
        [JsonProperty("count_100")] public int Count100 { get; set; }
        [JsonProperty("count_50")] public int Count50 { get; set; }
        [JsonProperty("count_geki")] public int CountGeki { get; set; }
        [JsonProperty("count_katu")] public int CountKatu { get; set; }
        [JsonProperty("count_miss")] public int CountMiss { get; set; }

        [JsonProperty("rank")] public ScoreRank Rank { get; set; }
        [JsonProperty("time")] public DateTime Time { get; set; }
        [JsonProperty("play_mode")] public GameMode PlayMode { get; set; }

        /// <summary>
        /// Accuracy of this play. 0-100.
        /// </summary>
        [JsonProperty("accuracy")] public double Accuracy { get; set; }
        [JsonProperty("pp")] public double PerformancePoints { get; set; }
        [JsonProperty("completed")] public int Completed { get; set; }
        [JsonProperty("beatmap")] public ScoreEmbeddedBeatmap Beatmap { get; set; }
    }
#pragma warning restore CS8618
}