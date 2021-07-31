using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OsuSharp;
using Pepper.Services.Osu.API;
using Pepper.Structures;
using Pepper.Structures.External.Osu;

namespace Pepper.Services.Osu
{
    public partial class APIService
    {
        private readonly BeatmapCache beatmapCache = new BeatmapCache();
        public async Task<WorkingBeatmap> GetBeatmap(long beatmapId) => await beatmapCache.GetBeatmap(beatmapId);
    }
}