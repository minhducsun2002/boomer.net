using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Beatmaps;

namespace Pepper.Services.Osu.API
{
    public class APIBeatmapSet : osu.Game.Online.API.Requests.Responses.APIBeatmapSet
    {
        private int? onlineBeatmapSetID;

        [JsonProperty(@"id")]
        public new int? OnlineBeatmapSetID
        {
            get => onlineBeatmapSetID;
            set => onlineBeatmapSetID = value > 0 ? value : null;
        }

        [JsonProperty(@"covers")] public BeatmapSetOnlineCovers Covers { get; set; }
        [JsonProperty(@"preview_url")] public string Preview { get; set; }
        [JsonProperty(@"has_favourited")] public bool HasFavourited { get; set; }
        [JsonProperty(@"play_count")] public int PlayCount { get; set; }
        [JsonProperty(@"favourite_count")] public int FavouriteCount { get; set; }
        [JsonProperty(@"bpm")] public double Bpm { get; set; }
        [JsonProperty(@"nsfw")] public bool HasExplicitContent { get; set; }
        [JsonProperty(@"video")] public bool HasVideo { get; set; }
        [JsonProperty(@"storyboard")] public bool HasStoryboard { get; set; }
        [JsonProperty(@"submitted_date")] public DateTimeOffset Submitted { get; set; }
        [JsonProperty(@"ranked_date")] public DateTimeOffset? Ranked { get; set; }
        [JsonProperty(@"last_updated")] public DateTimeOffset LastUpdated { get; set; }
        [JsonProperty(@"ratings")] public int[] Ratings = Array.Empty<int>();

        [JsonProperty(@"user_id")]
        public int CreatorId
        {
            set => Author.Id = value;
        }

        [JsonProperty(@"availability")] public BeatmapSetOnlineAvailability Availability { get; set; }
        [JsonProperty(@"genre")] public BeatmapSetOnlineGenre Genre { get; set; }
        [JsonProperty(@"language")] public BeatmapSetOnlineLanguage Language { get; set; }
        [JsonProperty(@"beatmaps")] public List<APIBeatmap> Beatmaps { get; set; }
        [JsonProperty(@"converts")] public List<APIBeatmap> Converts { get; set; }
    }
}