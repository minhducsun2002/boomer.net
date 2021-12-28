using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace Pepper.Commons.Osu.API
{
    public class BeatmapCompact : APIBeatmap
    {
        [JsonProperty("beatmapset")] public APIBeatmapSet BeatmapSet { get; set; }
    }
}