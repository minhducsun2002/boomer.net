using Pepper.Commons.Maimai.HtmlParsers;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        public async Task<User> GetUserPlayData()
        {
            var uid = await GetAuthUserId();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://maimaidx-eng.com/maimai-mobile/playerData/");
            request.Headers.TryAddWithoutValidation("Cookie", $"userId={uid}");
            var file = await httpClient.SendAsync(request);
            return PlayerDataParser.Parse(await file.Content.ReadAsStringAsync());
        }
    }
}