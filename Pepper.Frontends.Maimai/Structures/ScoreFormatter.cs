using Disqord;
using Humanizer;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Utils;
using Qommon;
using Difficulty = Pepper.Commons.Maimai.Entities.Difficulty;

namespace Pepper.Frontends.Maimai.Structures
{
    public class ScoreFormatter<TScoreType> where TScoreType : ScoreRecord
    {
        public static LocalEmbed FormatScore(ScoreWithMeta<TScoreType> record, int? number = null, bool showSongCategories = true)
        {
            (int, bool)? hint = record.Score is TopRecord top ? top.Level : null;
            return ScoreFormatter.FormatScore(record.Score, record.Difficulty, record.Song, number, record.ImageUrl, hint, record.SongHasMultipleVersion, showSongCategories);
        }
    }

    public class ScoreFormatter
    {
        public static readonly string[] DifficultyStrings =
        {
            "BASIC", "ADVANCED", "EXPERT", "MASTER", "Re:MASTER"
        };

        public static LocalEmbed FormatScore(
            ScoreRecord record,
            Difficulty? diff, Song? song,
            int? number = null,
            string? imageUrl = null,
            (int, bool)? levelHints = null,
            bool? hasMultipleVersions = null,
            bool showSongCategories = true)
        {
            var diffText = DifficultyStrings[(int) record.Difficulty];
            bool isAccurateLevel = diff != null, isTop = record is TopRecord;
            string levelText;
            int chartConstant;
            if (diff != null)
            {
                levelText = $"{diff.Level}.{diff.LevelDecimal}";
                chartConstant = diff.Level * 10 + diff.LevelDecimal;
            }
            else
            {
                if (levelHints.HasValue)
                {
                    var (baseLevel, plus) = levelHints.Value;
                    levelText = $"{baseLevel}{(plus ? "+" : "")}";
                    chartConstant = baseLevel * 10 + (plus ? 7 : 0);
                }
                else
                {
                    levelText = "";
                    chartConstant = default;
                }
            }

            if (levelText != "")
            {
                levelText = " " + levelText;
            }

            int rating = default;
            if (chartConstant != default)
            {
                rating = Calculate.NormalizedRating(Calculate.GetFinalScore(record.Accuracy, chartConstant));
            }

            var comboText = Format.Status(record.FcStatus);
            var syncText = Format.Status(record.SyncStatus);
            var nameText = record.Name + (record.Version == ChartVersion.Deluxe && hasMultipleVersions == true ? "  [DX] " : "  ");
            var ratingText = rating != default ? $" - **{rating}**{(isAccurateLevel ? "" : " (?)")} rating" : "";
            var rankText = $"**{record.Rank.ToUpperInvariant()}**{(record.RankPlus ? "+" : "")}";
            var numberText = number != null ? $"{number}. " : "";
            char openingBracket = isTop ? '(' : '[', closingBracket = isTop ? ')' : ']';

            var r = new LocalEmbed
            {
                Author = new LocalEmbedAuthor().WithName($"{numberText}{nameText}[{diffText}{levelText}]"),
                Description = $"**{record.Accuracy / 10000}**.**{record.Accuracy % 10000:0000}**%"
                              + (isTop ? $" - [{rankText}]" : $" - {rankText}")
                              + (comboText == "" ? comboText : $" {openingBracket}{comboText}{closingBracket}")
                              + (syncText == "" ? syncText : $" {openingBracket}{syncText}{closingBracket}")
                              + (isTop ? ratingText : ""),
                ThumbnailUrl = imageUrl ?? Optional<string>.Empty,
                Color = Format.Color(record.Difficulty)
            };

            var footerText = new List<string>();

            if (record is RecentRecord recentRecord)
            {
                r.Author = new LocalEmbedAuthor().WithName($"Track {number} - {diffText}{levelText}");
                r.Title = nameText;
                r.ThumbnailUrl = recentRecord.ImageUrl ?? Optional<string>.Empty;
                r.Timestamp = recentRecord.Timestamp;

                if (recentRecord.ChallengeType != ChallengeType.None)
                {
                    var hp = recentRecord.ChallengeRemainingHealth;
                    var maxHp = recentRecord.ChallengeMaxHealth;
#pragma warning disable CS8509
                    var text = recentRecord.ChallengeType switch
#pragma warning restore CS8509
                    {
                        ChallengeType.PerfectChallenge => $"Perfect Challenge : {hp}/{maxHp}",
                        ChallengeType.Course => $"Course : {hp}/{maxHp}"
                    };
                    footerText.Add(text);
                }
            }

            if (record is ChartRecord chartRecord)
            {
                var playCount = chartRecord.PlayCount;
                r.Description += $"\n\n{playCount} {(playCount < 2 ? "play" : "play".Pluralize())}, last played "
                    + $"<t:{chartRecord.LastPlayed.ToUnixTimeSeconds()}:f>";
            }

            if (rating != default && !isTop)
            {
                footerText.Add($"{rating}{(isAccurateLevel ? "" : " (?)")} rating");
            }

            if (footerText.Count != 0)
            {
                r = r.WithFooter(string.Join("  •  ", footerText));
            }

            if (song != null && showSongCategories)
            {
                r.AddField("Genre", song.Genre!.Name, true).AddField("Version", song.AddVersion!.Name, true);
                if (chartConstant != default)
                {
                    r.AddField("Level", $"{chartConstant / 10}{(chartConstant % 10 >= 7 ? "+" : "")}", true);
                }
            }

            return r;
        }
    }
}