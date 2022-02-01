using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace Pepper.Commons.Osu.API
{
#pragma warning disable CS8618
    public class BeatmapCompact : APIBeatmap
    {
        [JsonProperty("beatmapset")] public new APIBeatmapSet BeatmapSet { get; set; }
    }
#pragma warning restore CS8618
}