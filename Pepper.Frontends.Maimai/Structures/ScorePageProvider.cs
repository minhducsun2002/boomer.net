using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai.Structures.Data.Score;

namespace Pepper.Frontends.Maimai.Structures
{
    public abstract class ScorePageProvider<TScoreType> : PageProvider where TScoreType : ScoreRecord
    {
        private readonly List<ScoreWithMeta<TScoreType>> scores;

        protected ScorePageProvider(IEnumerable<ScoreWithMeta<TScoreType>> scoreRecords)
        {
            scores = scoreRecords.ToList();
        }
    }
}