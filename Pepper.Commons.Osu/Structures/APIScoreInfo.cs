using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API
{
    public class APIScoreInfo : osu.Game.Online.API.Requests.Responses.APIScoreInfo
    {
        [JsonProperty("perfect")]
        public bool Perfect { get; set; }

        [JsonProperty("best_id")]
        public long? OnlineBestScoreID { get; set; }
    }
}