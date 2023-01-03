using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.Maimai.Structures
{
    public class TopScorePagedView : PagedView
    {
        private readonly int oldIndex;

        private bool HasOld => oldIndex != 0;
        private bool HasNew => oldIndex < PageProvider.PageCount;
        private bool IsOld => CurrentPageIndex >= oldIndex;
        private bool IsNew => !IsOld;

        public TopScorePagedView(
            PageProvider pageProvider,
            int oldIndex,
            Action<LocalMessageBase>? messageTemplate = null
        ) : base(pageProvider, messageTemplate)
        {
            this.oldIndex = oldIndex;
            if (HasNew == HasOld)
            {
                AddComponent(new ButtonViewComponent(e =>
                {
                    CurrentPageIndex = IsNew ? oldIndex : default;
                    e.Button.Label = $"Switch to {(IsNew ? "old" : "new")}";
                    return ValueTask.CompletedTask;
                })
                {
                    Label = $"Switch to {(IsNew ? "old" : "new")}",
                    Style = LocalButtonComponentStyle.Secondary
                });
            }
        }
    }
}