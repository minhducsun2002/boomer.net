using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pepper.Commons.Osu.API
{
    [JsonObject(MemberSerialization.OptIn, ItemNullValueHandling = NullValueHandling.Ignore)]
    public class APIScore : osu.Game.Online.API.Requests.Responses.SoloScoreInfo
    {
        [JsonProperty("legacy_perfect")]
        public bool Perfect { get; set; }

        [JsonProperty("best_id")]
        public long? OnlineBestScoreID { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public new ScoreRank Rank { get; set; }

        public DateTimeOffset Date => EndedAt;
    }
}