using System;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Pepper.Structures
{
    public class PagedViewBase : Disqord.Extensions.Interactivity.Menus.Paged.PagedViewBase
    {
        protected PagedViewBase(PageProvider pageProvider, Action<LocalMessageBase>? messageTemplate = null) : base(pageProvider, messageTemplate) { }

        public override async ValueTask DisposeAsync()
        {
            foreach (var component in EnumerateComponents())
            {
                switch (component)
                {
                    case ButtonViewComponent buttonViewComponent:
                        {
                            buttonViewComponent.IsDisabled = true;
                            break;
                        }
                    case SelectionViewComponent selectionViewComponent:
                        {
                            selectionViewComponent.IsDisabled = true;
                            break;
                        }
                }
            }
            await Menu.ApplyChangesAsync();
            await base.DisposeAsync();
        }
    }
}