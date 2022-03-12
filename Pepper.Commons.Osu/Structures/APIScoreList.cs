using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API
{
#pragma warning disable CS8618
    public class APIScoreList
    {
        [JsonProperty("scores")] public APIScore[] Scores { get; set; }
    }
#pragma warning restore CS8618
}