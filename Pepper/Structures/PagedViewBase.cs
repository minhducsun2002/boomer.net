using System;
using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Pepper.Structures
{
    // Override the default behaviour of PagedView.
    public class PagedViewBase : PagedView
    {
        protected PagedViewBase(PageProvider pageProvider, Action<LocalMessageBase>? messageTemplate = null) : base(pageProvider, messageTemplate)
        {
            ClearComponents();
        }

        protected override void ApplyPageIndex(Page page) { }
    }
}