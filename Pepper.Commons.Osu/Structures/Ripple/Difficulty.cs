using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API.Ripple
{
    internal class Difficulty
    {
        [JsonProperty("std")] public double Standard { get; set; }
        [JsonProperty("taiko")] public double Taiko { get; set; }
        [JsonProperty("ctb")] public double Catch { get; set; }
        [JsonProperty("mania")] public double Mania { get; set; }
    }
}