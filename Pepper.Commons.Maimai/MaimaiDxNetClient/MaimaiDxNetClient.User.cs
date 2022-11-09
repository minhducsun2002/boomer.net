using Pepper.Commons.Maimai.HtmlParsers;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        public async Task<User> GetUserPlayData()
        {
            var html = await GetHtml("https://maimaidx-eng.com/maimai-mobile/playerData/");
            return PlayerDataParser.Parse(html);
        }
    }
}