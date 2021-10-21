using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;
using OsuSharp;
using Pepper.Services.Osu.API;
using Pepper.Structures;
using Pepper.Structures.External.Osu;
using APIBeatmapSet = Pepper.Services.Osu.API.APIBeatmapSet;

namespace Pepper.Services.Osu
{
    public partial class APIService
    {
        private readonly BeatmapCache beatmapCache = new BeatmapCache();
        private readonly BeatmapsetMetadataCache beatmapsetMetadataCache = new();

        public Task<WorkingBeatmap> GetBeatmap(int beatmapId) => beatmapCache.GetBeatmap(beatmapId);
        public Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId) =>
            beatmapsetMetadataCache.GetBeatmapsetInfo(id, isBeatmapSetId);
    }
}