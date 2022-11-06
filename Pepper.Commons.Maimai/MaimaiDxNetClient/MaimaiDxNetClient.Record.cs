using Pepper.Commons.Maimai.HtmlParsers;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        public async Task<IEnumerable<RecentRecord>> GetUserRecentRecord()
        {
            var uid = await GetAuthUserId();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://maimaidx-eng.com/maimai-mobile/record/");
            request.Headers.TryAddWithoutValidation("Cookie", $"userId={uid}");
            var file = await httpClient.SendAsync(request);
            return RecentRecordParser.Parse(await file.Content.ReadAsStringAsync());
        }
    }
}