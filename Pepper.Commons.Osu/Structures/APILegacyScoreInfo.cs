using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API
{
    public class APILegacyScoreInfo : osu.Game.Online.API.Requests.Responses.APILegacyScoreInfo
    {
        [JsonProperty("perfect")]
        public bool Perfect { get; set; }

        [JsonProperty("best_id")]
        public long? OnlineBestScoreID { get; set; }
    }
}