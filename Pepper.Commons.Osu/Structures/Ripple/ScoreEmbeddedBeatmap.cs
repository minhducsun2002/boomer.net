using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API.Ripple
{
#pragma warning disable CS8618
    internal class ScoreEmbeddedBeatmap
    {
        [JsonProperty("beatmap_id")] public int Id { get; set; }
        [JsonProperty("beatmapset_id")] public int SetId { get; set; }
        [JsonProperty("beatmap_md5")] public string Md5 { get; set; }
        [JsonProperty("song_name")] public string Title { get; set; }
        [JsonProperty("max_combo")] public int MaxCombo { get; set; }
        [JsonIgnore] public Dictionary<GameMode, double> Difficulty = new();

        // [JsonProperty("ar")] public int Ar { get; set; }
        // [JsonProperty("od")] public int Od { get; set; }

        // [JsonProperty("difficulty")] public int Difficulty { get; set; }
        [JsonProperty("difficulty2")]
        private Difficulty Difficulty2
        {
            set
            {
                Difficulty[GameMode.Standard] = value.Standard;
                Difficulty[GameMode.Taiko] = value.Taiko;
                Difficulty[GameMode.Catch] = value.Catch;
                Difficulty[GameMode.Mania] = value.Mania;
            }
        }

        [JsonProperty("hit_length")] public int HitLength { get; set; }
        // [JsonProperty("ranked")] public int Ranked { get; set; }
        // [JsonProperty("ranked_status_frozen")] public int RankedStatusFrozen { get; set; }
        [JsonProperty("latest_update")] public DateTime LatestUpdate { get; set; }
    }
#pragma warning restore CS8618
}