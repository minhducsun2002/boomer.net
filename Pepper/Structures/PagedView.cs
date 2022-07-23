using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Pepper.Structures
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
    }

    public class SelectionPagedView : PagedViewBase
    {
        private readonly Dictionary<int, LocalSelectionComponentOption> details;
        public SelectionPagedView(IEnumerable<(LocalSelectionComponentOption, Page)> pages, Action<LocalMessageBase>? messageTemplate = null)
            : base(PreparePages(pages, out var pageDetails), messageTemplate)
        {
            details = pageDetails;
            var selection = new SelectionViewComponent(e =>
            {
                var index = int.Parse(e.SelectedOptions[0].Value.Value);
                CurrentPageIndex = index;
                e.Selection.Options = GetCurrentOptions();
                return UpdateAsync();
            })
            {
                MaximumSelectedOptions = 1,
                MinimumSelectedOptions = 1,
                Options = GetCurrentOptions()
            };

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

        private static ListPageProvider PreparePages(IEnumerable<(LocalSelectionComponentOption, Page)> pages, out Dictionary<int, LocalSelectionComponentOption> details)
        {
            var orderedPages = pages
                .Select((pair, index) => (pair.Item1, pair.Item2, index)).ToArray();
            var pageProvider = new ListPageProvider(orderedPages.Select(tuple => tuple.Item2));
            var indexedDetails = orderedPages.ToDictionary(
                tuple => tuple.index,
                tuple => tuple.Item1
            );

            details = indexedDetails;

            return pageProvider;
        }
    }
}