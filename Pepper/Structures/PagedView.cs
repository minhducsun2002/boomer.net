using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Pepper.Structures
{
    public class PagedView : Disqord.Extensions.Interactivity.Menus.Paged.PagedView
    {
        public PagedView(PageProvider pageProvider, LocalMessage? templateMessage = null) : base(pageProvider, templateMessage)
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
}