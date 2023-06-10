using Disqord;
using Humanizer;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Structures;

namespace Pepper.Frontends.Maimai.Utils
{
    public static class Format
    {
        public static string DanLevel(int danLevel)
        {
            var isShinDan = danLevel > 11;
            return $"{(isShinDan ? "Shin " : "")}{(danLevel % 11).Ordinalize()} Dan";
        }

        public static string SongName<T>(ScoreWithMeta<T> sc) where T : ScoreRecord
        {
            var record = sc.Score;
            return record.Name + (record.Version == ChartVersion.Deluxe && sc.SongHasMultipleVersion ? "  [DX] " : "  ");
        }

        public static string ChartConstant<T>(ScoreWithMeta<T> sc, (int, bool)? levelHints = null) where T : ScoreRecord
        {
            var diff = sc.Difficulty;
            string levelText;
            if (diff != null)
            {
                levelText = $"{diff.Level}.{diff.LevelDecimal}";
            }
            else
            {
                if (levelHints.HasValue)
                {
                    var (baseLevel, plus) = levelHints.Value;
                    levelText = $"{baseLevel}{(plus ? "+" : "")}";
                }
                else
                {
                    levelText = "";
                }
            }

            return levelText;
        }

        public static string Statistics(ScoreRecord record)
        {
            var isTop = record is TopRecord;
            var comboText = Status(record.FcStatus);
            var syncText = Status(record.SyncStatus);
            var rankText = $"**{record.Rank.ToUpperInvariant()}**{(record.RankPlus ? "+" : "")}";
            char openingBracket = isTop ? '(' : '[', closingBracket = isTop ? ')' : ']';
            return $"**{record.Accuracy / 10000}**.**{record.Accuracy % 10000:0000}**%"
                   + (isTop ? $" - [{rankText}]" : $" - {rankText}")
                   + (comboText == "" ? comboText : $" {openingBracket}{comboText}{closingBracket}")
                   + (syncText == "" ? syncText : $" {openingBracket}{syncText}{closingBracket}");
        }

        public static string Rating<T>(ScoreWithMeta<T> sc) where T : ScoreRecord
        {
            var rating = sc.Rating;
            if (rating is not null)
            {
                rating = Calculate.NormalizedRating(rating.Value);
            }
            return rating != null ? $"**{rating}**{(sc.IsRatingAccurate ? "" : " (?)")}" : "";
        }

        public static string Status(FcStatus fcStatus)
        {
            var comboText = fcStatus switch
            {
                FcStatus.FC => "**FC**",
                FcStatus.FCPlus => "**FC**+",
                FcStatus.AllPerfect => "**AP**",
                FcStatus.AllPerfectPlus => "**AP**+",
                _ => ""
            };
            return comboText;
        }

        public static string Status(SyncStatus syncStatus)
        {
            var syncText = syncStatus switch
            {
                SyncStatus.FullSyncDx => "**FS DX**",
                SyncStatus.FullSyncDxPlus => "**FS DX**+",
                SyncStatus.FullSync => "**FS**",
                SyncStatus.FullSyncPlus => "**FS**+",
                _ => ""
            };
            return syncText;
        }

        public static Color Color(Difficulty difficulty)
        {
            var color = difficulty switch
            {
                Difficulty.Basic => new Color(0x6fe163),
                Difficulty.Advanced => new Color(0xf8df3a),
                Difficulty.Expert => new Color(0xff828e),
                Difficulty.Master => new Color(0xc27ff4),
                Difficulty.ReMaster => new Color(0xe5ddea),
                _ => new Color(0x6fe163)
            };
            return color;
        }
    }
}