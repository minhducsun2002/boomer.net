using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API.Ripple
{
    internal class RippleUserStatistics
    {
        [JsonProperty("ranked_score")] public int RankedScore { get; set; }
        [JsonProperty("total_score")] public int TotalScore { get; set; }
        [JsonProperty("playcount")] public int PlayCount { get; set; }
        [JsonProperty("play_time")] public int PlayTimeInSeconds { get; set; }
        [JsonProperty("replays_watched")] public int ReplaysWatched { get; set; }
        [JsonProperty("total_hits")] public int TotalHits { get; set; }
        [JsonProperty("level")] public double Level { get; set; }
        /// <summary>
        /// Total Accuracy. A percentage (i.e. ranges from 0 to 100 inclusively). 
        /// </summary>
        [JsonProperty("accuracy")] public double Accuracy { get; set; }
        [JsonProperty("pp")] public int PerformancePoints { get; set; }
        [JsonProperty("global_leaderboard_rank")] public int? GlobalLeaderboardRank { get; set; }
        [JsonProperty("country_leaderboard_rank")] public int? CountryLeaderboardRank { get; set; }
    }
}