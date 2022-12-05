using Pepper.Commons.Maimai.HtmlParsers;
using Pepper.Commons.Maimai.Structures;

namespace Pepper.Commons.Maimai
{
    public partial class MaimaiDxNetClient
    {
        public async Task<IEnumerable<RecentRecord?>> GetUserRecentRecord()
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

        public async Task<IEnumerable<ScoreRecord>> GetUserDifficultyRecord((int, bool) level)
        {
            var (lv, plus) = level;
            if (lv is < 1 or > 15)
            {
                throw new ArgumentOutOfRangeException(nameof(level), lv, "should be from 1 to 15");
            }

            var option = plus ? levelOptions[lv - 1].Item3 : levelOptions[lv - 1].Item2;
            var url = $"https://maimaidx-eng.com/maimai-mobile/record/musicLevel/search/?level={option}";
            var html = await GetHtml(url);
            return AllRecordParser.Parse(html);
        }

        private readonly (int, int, int)[] levelOptions =
        {
            // level, option value if base, option value if plus
            (1, 1, 1),
            (2, 2, 2),
            (3, 3, 3),
            (4, 4, 4),
            (5, 5, 5),
            (6, 6, 6),
            (7, 7, 8),
            (8, 9, 10),
            (9, 11, 12),
            (10, 13, 14),
            (11, 15, 16),
            (12, 17, 18),
            (13, 19, 20),
            (14, 21, 22),
            // make it all 23 just in case though the 2nd value supposed to be 24
            (15, 23, 23),
        };
    }
}