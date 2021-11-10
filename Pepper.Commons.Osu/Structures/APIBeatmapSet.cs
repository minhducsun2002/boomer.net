using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;

namespace Pepper.Commons.Osu.API
{
    public class APIBeatmapSet : osu.Game.Online.API.Requests.Responses.APIBeatmapSet
    {
        [JsonProperty(@"converts")] public List<APIBeatmap> Converts { get; set; }
    }
}