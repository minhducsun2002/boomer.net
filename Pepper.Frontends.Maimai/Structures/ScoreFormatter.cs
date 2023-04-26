using Disqord;
using Humanizer;
using Pepper.Commons.Maimai.Structures.Data.Enums;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Utils;
using Qommon;

namespace Pepper.Frontends.Maimai.Structures
{
    public class ScoreFormatter<TScoreType> where TScoreType : ScoreRecord
    {
        public static LocalEmbed FormatScore(ScoreWithMeta<TScoreType> record, int? number = null, bool showSongCategories = true)
        {
            (int, bool)? hint = record.Score is TopRecord top ? top.Level : null;
            return ScoreFormatter.FormatScore(record, number, record.ImageUrl, hint, showSongCategories);
        }
    }

    public class ScoreFormatter
    {
        public static readonly string[] DifficultyStrings =
        {
            "BASIC", "ADVANCED", "EXPERT", "MASTER", "Re:MASTER"
        };

        public static LocalEmbed FormatScore<TScoreType>(
            ScoreWithMeta<TScoreType> e,
            int? number = null,
            string? imageUrl = null,
            (int, bool)? levelHints = null,
            bool showSongCategories = true) where TScoreType : ScoreRecord

        {
            var record = e.Score;
            var diff = e.Difficulty;
            var song = e.Song;
            var diffText = DifficultyStrings[(int) record.Difficulty];
            bool isAccurateLevel = diff != null, isTop = record is TopRecord;
            int chartConstant;
            if (diff != null)
            {
                chartConstant = diff.Level * 10 + diff.LevelDecimal;
            }
            else
            {
                if (levelHints.HasValue)
                {
                    var (baseLevel, plus) = levelHints.Value;
                    chartConstant = baseLevel * 10 + (plus ? 7 : 0);
                }
                else
                {
                    chartConstant = default;
                }
            }

            int rating = default;
            if (chartConstant != default)
            {
                rating = Calculate.NormalizedRating(Calculate.GetFinalScore(record.Accuracy, chartConstant));
            }

            var nameText = Format.SongName(e);
            var ratingText = rating != default ? $" - **{rating}**{(isAccurateLevel ? "" : " (?)")} rating" : "";
            var numberText = number != null ? $"{number}. " : "";
            var levelText = Format.ChartConstant(e, levelHints);
            if (!string.IsNullOrEmpty(levelText))
            {
                levelText = " " + levelText;
            }

            var r = new LocalEmbed
            {
                Author = new LocalEmbedAuthor().WithName($"{numberText}{nameText}[{diffText}{levelText}]"),
                Description = Format.Statistics(record) + (isTop ? ratingText : ""),
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