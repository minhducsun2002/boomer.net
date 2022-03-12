using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pepper.Commons.Osu.API
{
    [JsonObject(MemberSerialization.OptIn)]
    public class APIScore : osu.Game.Online.API.Requests.Responses.APIScore
    {
        [JsonProperty("perfect")]
        public bool Perfect { get; set; }

        [JsonProperty("best_id")]
        public long? OnlineBestScoreID { get; set; }

        [JsonProperty("rank")]
        [JsonConverter(typeof(StringEnumConverter))]
        public new ScoreRank Rank { get; set; }
    }
}