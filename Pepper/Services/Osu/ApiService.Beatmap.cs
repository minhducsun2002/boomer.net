using System.Threading.Tasks;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu.API;

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