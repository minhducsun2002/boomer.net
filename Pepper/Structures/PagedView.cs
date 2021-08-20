using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
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

    public class SelectionPagedView : PagedViewBase
    {
        private readonly Dictionary<int, LocalSelectionComponentOption> details;
        public SelectionPagedView(Dictionary<(int, LocalSelectionComponentOption), Page> pages, LocalMessage? templateMessage = null) 
            : base(PreparePages(pages, out var pageDetails), templateMessage)
        {
            var selection = new SelectionViewComponent(e =>
            {
                var index = int.Parse(e.SelectedOptions[0].Value);
                CurrentPageIndex = index;
                e.Selection.Options = GetCurrentOptions();
                return UpdateAsync();
            })
            {
                MaximumSelectedOptions = 1,
                MinimumSelectedOptions = 1,
                Options = GetCurrentOptions()
            };

            details = pageDetails;
            
            AddComponent(selection);
        }

        private List<LocalSelectionComponentOption> GetCurrentOptions()
            => details.OrderBy(kv => kv.Key)
                .Select(kv =>
                {
                    var (index, option) = kv;
                    return new LocalSelectionComponentOption
                    {
                        IsDefault = CurrentPageIndex == index,
                        Label = option.Label,
                        Value = $"{index}",
                        Description = option.Description,
                        Emoji = option.Emoji
                    };
                })
                .ToList();

        private static ListPageProvider PreparePages(Dictionary<(int, LocalSelectionComponentOption), Page> pages, out Dictionary<int, LocalSelectionComponentOption> details)
        {
            var orderedPages = pages
                .OrderBy(kv => kv.Key.Item1)
                .Select((kv, index) => (kv.Key.Item2, kv.Value, index)).ToArray();
            var pageProvider = new ListPageProvider(orderedPages.Select(tuple => tuple.Value));
            var indexedDetails = orderedPages.ToDictionary(
                tuple => tuple.index,
                tuple => tuple.Item1
            );

            details = indexedDetails;
            
            return pageProvider;
        }
    }
}