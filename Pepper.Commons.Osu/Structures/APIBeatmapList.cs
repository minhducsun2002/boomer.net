using Newtonsoft.Json;

namespace Pepper.Commons.Osu.API
{
#pragma warning disable CS8618
    /// <summary>
    /// Response returned from the <code>/beatmaps?id[]=</code> endpoint when looking up multiple beatmaps. 
    /// </summary>
    public class APIBeatmapList
    {
        [JsonProperty("beatmaps")] public BeatmapCompact[] Beatmaps { get; set; }
    }
#pragma warning restore CS8618
}