#nullable enable
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using LazyCache;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Users;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Color = System.Drawing.Color;

namespace Pepper.Services.Osu.API
{
    internal class UserCache
    {
        private const int UserCachingDurationSeconds = 15;
        private const int UserAvatarMainColorCachingDurationSeconds = 30 * 60;

        private readonly IAppCache userObjectCache, userAvatarCache;
        private static readonly HttpClient HttpClient = new ();
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        internal UserCache(
            int userCachingDurationSeconds = UserCachingDurationSeconds,
            int userAvatarMainColorCachingDurationSeconds = UserAvatarMainColorCachingDurationSeconds)
        {
            userObjectCache = new CachingService
            {
                DefaultCachePolicy = new CacheDefaults
                    {DefaultCacheDurationSeconds = userCachingDurationSeconds}
            };
            userAvatarCache = new CachingService
            {
                DefaultCachePolicy = new CacheDefaults
                    {DefaultCacheDurationSeconds = userAvatarMainColorCachingDurationSeconds}
            };
        }
        
        public async Task<(User, APILegacyScoreInfo[])> Get(string username, RulesetInfo rulesetInfo)
        {
            var rulesetName = rulesetInfo.ShortName;

            async Task<(User, APILegacyScoreInfo[])> UserGetter()
            {
                var _ = await HttpClient.GetStringAsync($"https://osu.ppy.sh/users/{HttpUtility.UrlPathEncode(username)}/{rulesetName}");
                var doc = new HtmlDocument(); doc.LoadHtml(_);
                var user = JsonConvert.DeserializeObject<User>(doc.GetElementbyId("json-user").InnerText, SerializerSettings)!;
                var scores = JObject.Parse(doc.GetElementbyId("json-extras").InnerText)["scoresBest"]!
                    .ToArray().Select(token =>
                    {
                        var legacyScoreInfo = token.ToObject<APILegacyScoreInfo>();
                        var jsonBeatmapInfo = token["beatmap"]!;
                        legacyScoreInfo!.Beatmap.BaseDifficulty = new BeatmapDifficulty
                        {
                            ApproachRate = jsonBeatmapInfo["ar"]!.ToObject<float>(),
                            CircleSize = jsonBeatmapInfo["cs"]!.ToObject<float>(),
                            DrainRate = jsonBeatmapInfo["drain"]!.ToObject<float>(),
                            OverallDifficulty = jsonBeatmapInfo["accuracy"]!.ToObject<float>()
                        };
                        legacyScoreInfo!.Beatmap.Length = jsonBeatmapInfo["hit_length"]!.ToObject<int>() * 1000;
                        return legacyScoreInfo;
                    }).ToArray();
                Log.Debug($"Caching records for user {SerializeUserInLog(user)} for {UserCachingDurationSeconds}s.");
                return (user, scores);
            }

            return await userObjectCache.GetOrAddAsync($"osu-user-{username.ToLowerInvariant()}-{rulesetName}", UserGetter);
        }

        /**
         * Used to set the embed color of o!user
         */
        public async Task<Color> GetUserAvatarDominantColor(User user)
        {
            async Task<Color> UserAvatarGetter()
            {
                var avatar = await HttpClient.GetByteArrayAsync(user.AvatarUrl);
                var image = Image.Load(avatar);
                image.Mutate(img => img.Quantize(new OctreeQuantizer(new QuantizerOptions { MaxColors = 2 }))
                    .Resize(1, 1, KnownResamplers.Bicubic));
                var color = image[0, 0];
                Log.Debug(
                    $"Caching profile picture main color for user {SerializeUserInLog(user)} for {UserAvatarMainColorCachingDurationSeconds}s."
                );
                return Color.FromArgb(color.R, color.G, color.B);
            }

            return await userAvatarCache.GetOrAddAsync($"osu-user-avatar-{user.Id}", UserAvatarGetter);
        }

        private static string SerializeUserInLog(User user) => $"{user.Id} - \"{user.Username}\"";
    }
}