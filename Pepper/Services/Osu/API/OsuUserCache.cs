#nullable enable
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using BitFaster.Caching.Lru;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using APIScoreInfo = Pepper.Commons.Osu.API.APIScoreInfo;
using Color = System.Drawing.Color;

namespace Pepper.Services.Osu.API
{
    internal class OsuUserCache
    {
        private const int UserAvatarMainColorCachingDurationSeconds = 30 * 60;

        private readonly FastConcurrentTLru<string, Color> userAvatarCache = new(
            200, TimeSpan.FromSeconds(UserAvatarMainColorCachingDurationSeconds)
        );

        private static readonly HttpClient HttpClient = new();
        private static readonly JsonSerializerSettings SerializerSettings = new() { NullValueHandling = NullValueHandling.Ignore };
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<OsuUserCache>();

        public async Task<(APIUser, APIScoreInfo[])> Get(string username, RulesetInfo rulesetInfo)
        {
            var rulesetName = rulesetInfo.ShortName;
            var key = $"osu-user-{username.ToLowerInvariant()}-{rulesetName}";

            var _ = await HttpClient.GetStringAsync($"https://osu.ppy.sh/users/{HttpUtility.UrlPathEncode(username)}/{rulesetName}");
            var doc = new HtmlDocument(); doc.LoadHtml(_);
            var user = JsonConvert.DeserializeObject<APIUser>(doc.GetElementbyId("json-user").InnerText, SerializerSettings)!;
            var scores = JObject.Parse(doc.GetElementbyId("json-extras").InnerText)["scoresBest"]!
                .ToArray().Select(token =>
                {
                    var apiScoreInfo = token.ToObject<APIScoreInfo>();
                    var jsonBeatmapInfo = token["beatmap"]!;
                    apiScoreInfo!.Beatmap.Length = jsonBeatmapInfo["hit_length"]!.ToObject<int>() * 1000;
                    return apiScoreInfo;
                }).ToArray();

            return (user, scores);
        }

        /**
         * Used to set the embed color of o!user
         */
        public async Task<Color> GetUserAvatarDominantColor(APIUser user)
        {
            var key = $"osu-user-avatar-{user.Id}";
            if (userAvatarCache.TryGet(key, out var @return))
            {
                return @return;
            }

            var avatar = await HttpClient.GetByteArrayAsync(user.AvatarUrl);
            var image = Image.Load(avatar);
            image.Mutate(img => img.Quantize(new OctreeQuantizer(new QuantizerOptions { MaxColors = 2 }))
                .Resize(1, 1, KnownResamplers.Bicubic));
            var color = image[0, 0];
            Log.Debug(
                $"Caching profile picture main color for user {SerializeUserInLog(user)} for {UserAvatarMainColorCachingDurationSeconds}s."
            );
            var result = Color.FromArgb(color.R, color.G, color.B);
            userAvatarCache.AddOrUpdate(key, result);

            return result;
        }

        private static string SerializeUserInLog(APIUser user) => $"{user.Id} - \"{user.Username}\"";
    }
}