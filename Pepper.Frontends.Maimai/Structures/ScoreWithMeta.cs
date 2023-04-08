using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Utils;

namespace Pepper.Frontends.Maimai.Structures
{
    public class ScoreWithMeta<TScoreType> where TScoreType : ScoreRecord
    {
        // FESTiVAL
        public ScoreWithMeta(TScoreType score, Song? song, Difficulty? difficulty, int version = 19, bool hasMultipleVersions = true, string? imageUrl = null)
        {
            Score = score;
            Song = song;
            Difficulty = difficulty;
            SongHasMultipleVersion = hasMultipleVersions;
            ImageUrl = imageUrl;

            Version = song?.AddVersionId ?? version;
        }

        public TScoreType Score { get; }
        public Song? Song { get; }
        public Difficulty? Difficulty { get; }
        public int Version { get; }
        public bool SongHasMultipleVersion { get; }
        public string? ImageUrl { get; }

        public bool IsRatingAccurate => IsConstantAccurate;
        public bool IsConstantAccurate => Difficulty != null;

        public int? ChartConstant
        {
            get
            {
                var d = Difficulty;
                int p1, p2;
                if (d is not null)
                {
                    p1 = d.Level;
                    p2 = d.LevelDecimal;
                }
                else
                {
                    if (Score is not TopRecord top)
                    {
                        return null;
                    }
                    var (level, plus) = top.Level;
                    p1 = level;
                    p2 = plus ? 7 : 0;
                }

                return p1 * 10 + p2;
            }
        }
        public long? Rating
        {
            get
            {
                var c = ChartConstant;
                return c == null ? null : Calculate.GetFinalScore(Score.Accuracy, c.Value);
            }
        }
    }
}