using System;
using System.Threading.Tasks;
using System.Web;
using BitFaster.Caching.Lru;
using HtmlAgilityPack;
using Newtonsoft.Json;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Quantization;
using Color = System.Drawing.Color;

namespace Pepper.Commons.Osu.APIClients.Default
{
    public partial class DefaultOsuAPIClient
    {
        private readonly FastConcurrentTLru<string, Color> userColorCache = new(200, TimeSpan.FromSeconds(30 * 60));

        public async Task<APIUser> GetUser(string username, RulesetInfo rulesetInfo)
        {
            var rulesetName = rulesetInfo.ShortName;
            var html = await httpClient.GetStringAsync($"https://osu.ppy.sh/users/{HttpUtility.UrlPathEncode(username)}/{rulesetName}");
            var doc = new HtmlDocument(); doc.LoadHtml(html);
            var user = JsonConvert.DeserializeObject<APIUser>(
                doc.GetElementbyId("json-user").InnerText,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }
            )!;
            return user;
        }

        public async Task<Color> GetUserColor(APIUser user)
        {
            var key = $"osu-user-avatar-{user.Id}";
            if (userColorCache.TryGet(key, out var @return))
            {
                return @return;
            }

            var avatar = await httpClient.GetByteArrayAsync(user.AvatarUrl);
            var image = Image.Load(avatar);
            image.Mutate(img => img.Quantize(new OctreeQuantizer(new QuantizerOptions { MaxColors = 2 }))
                .Resize(1, 1, KnownResamplers.Bicubic));
            var color = image[0, 0];
            var result = Color.FromArgb(color.R, color.G, color.B);
            userColorCache.AddOrUpdate(key, result);

            return result;
        }
    }
}