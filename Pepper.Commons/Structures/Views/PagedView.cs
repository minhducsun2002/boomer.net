using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Pepper.Commons.Structures.Views
{
    public class PagedView : Disqord.Extensions.Interactivity.Menus.Paged.PagedView
    {
        public PagedView(PageProvider pageProvider, Action<LocalMessageBase>? messageTemplate = null) : base(pageProvider, messageTemplate)
        {
            if (pageProvider.PageCount <= 1)
            {
                ClearComponents();
            }
            else
            {
                RemoveComponent(StopButton);
                FirstPageButton.Label = "<<";
                FirstPageButton.Emoji = null;
                PreviousPageButton.Label = "<";
                PreviousPageButton.Emoji = null;
                NextPageButton.Label = ">";
                NextPageButton.Emoji = null;
                LastPageButton.Label = ">>";
                LastPageButton.Emoji = null;
            }
        }

        protected override void ApplyPageIndex(Page page)
        {
            if (PageProvider.PageCount > 1)
            {
                base.ApplyPageIndex(page);
            }
        }

        public override async ValueTask DisposeAsync()
        {
            ClearComponents();
            // TODO: Remove all this try-catch
            try
            {
                await Menu.ApplyChangesAsync();
            }
            catch (Exception e)
            {
                // ignored for now
            }
            await base.DisposeAsync();
        }
    }
}