using Pepper.Commons.Maimai;
using Pepper.Commons.Maimai.Entities;
using Pepper.Commons.Maimai.Structures.Data.Score;
using Pepper.Frontends.Maimai.Utils;

namespace Pepper.Frontends.Maimai.Structures
{
    public class ScoreWithMeta<TScoreType> where TScoreType : ScoreRecord
    {
        // FESTiVAL
        public ScoreWithMeta(TScoreType score, ISong? song, ChartLevel? level, int version = 19, bool hasMultipleVersions = true, string? imageUrl = null)
        {
            Score = score;
            Song = song;
            if (level is not null)
            {
                Level = level;
            }
            SongHasMultipleVersion = hasMultipleVersions;
            ImageUrl = imageUrl;

            AddVersion = song?.AddVersionId ?? version;
        }

        public TScoreType Score { get; }
        public ISong? Song { get; }
        public ChartLevel? Level { get; }
        public int AddVersion { get; }
        public bool SongHasMultipleVersion { get; }
        public string? ImageUrl { get; }

        public bool IsRatingAccurate => IsConstantAccurate;
        public bool IsConstantAccurate => Level != null;

        public int? ChartConstant
        {
            get
            {
                var d = Level;
                int p1, p2;
                if (d.HasValue)
                {
                    p1 = d.Value.Whole;
                    p2 = d.Value.Decimal;
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