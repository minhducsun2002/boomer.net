using System.Net.Http;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using HtmlAgilityPack;
using Newtonsoft.Json;
using osu.Game.Beatmaps;
using Pepper.Commons.Osu.API;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        private readonly FastConcurrentLru<int, WorkingBeatmap> beatmapCache = new(200);
        private readonly FastConcurrentLru<string, APIBeatmapSet> beatmapsetMetadataCache = new(200);
        public override async Task<WorkingBeatmap> GetBeatmap(int beatmapId)
        {
            if (beatmapCache.TryGet(beatmapId, out var @return))
            {
                return @return;
            }

            var file = await HttpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{beatmapId}");
            var result = WorkingBeatmap.Decode(file, beatmapId);

            if (result.BeatmapInfo.BeatmapSet?.OnlineID is null)
            {
                var beatmapsetInfo = await GetBeatmapsetInfo(result.BeatmapInfo.OnlineID, false);

                result.BeatmapInfo.BeatmapSet ??= new BeatmapSetInfo();
                result.BeatmapInfo.BeatmapSet.OnlineID = beatmapsetInfo.OnlineID;
            }

            beatmapCache.TryUpdate(beatmapId, result);
            return result;
        }

        public override async Task<APIBeatmapSet> GetBeatmapsetInfo(long id, bool isBeatmapSetId)
        {
            var key = id + "-" + (isBeatmapSetId ? "1" : "0");
            if (beatmapsetMetadataCache.TryGet(key, out var @return))
            {
                return @return;
            }

            var obj = await GetFreshBeatmapsetInfo(HttpClient, id, isBeatmapSetId);

            beatmapsetMetadataCache.AddOrUpdate(key, obj);
            return obj;
        }

        internal static async Task<APIBeatmapSet> GetFreshBeatmapsetInfo(HttpClient httpClient, long id, bool isBeatmapSetId)
        {
            var res = await httpClient.GetStringAsync($"https://osu.ppy.sh/{(isBeatmapSetId ? "beatmapsets" : "beatmaps")}/{id}");

            var doc = new HtmlDocument(); doc.LoadHtml(res);
            return JsonConvert.DeserializeObject<APIBeatmapSet>(doc.GetElementbyId("json-beatmapset").InnerText)!;
        }
    }
}