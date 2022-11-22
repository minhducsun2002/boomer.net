using Pepper.Commons.Maimai.HtmlParsers;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        public async Task<IEnumerable<RecentRecord>> GetUserRecentRecord()
        {
            var html = await GetHtml("https://maimaidx-eng.com/maimai-mobile/record/");
            return RecentRecordParser.Parse(html);
        }

        public async Task<IEnumerable<ScoreRecord>> GetUserDifficultyRecord(Difficulty difficulty)
        {
            var url =
                $"https://maimaidx-eng.com/maimai-mobile/record/musicGenre/search/?genre=99&diff={(int) difficulty}";
            var html = await GetHtml(url);
            return AllRecordParser.Parse(html);
        }
    }
}