using Disqord.Extensions.Interactivity.Menus.Paged;
using Pepper.Commons.Maimai.Structures.Data.Score;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.Maimai.Structures
{
    public abstract class ScorePagedView<TScoreType> : PagedView where TScoreType : ScoreRecord
    {
        protected ScorePagedView(PageProvider pageProvider) : base(pageProvider) { }
    }
}