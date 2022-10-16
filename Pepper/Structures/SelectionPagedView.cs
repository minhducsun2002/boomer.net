using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;

namespace Pepper.Structures
{
    public class SelectionPagedView : PagedViewBase
    {
        private readonly Dictionary<int, LocalSelectionComponentOption> details;
        public SelectionPagedView(
            IReadOnlyList<(LocalSelectionComponentOption, Page)> pages,
            int initialPageIndex = 0,
            Action<LocalMessageBase>? messageTemplate = null)
            : base(PreparePages(pages, out var pageDetails), messageTemplate)
        {
            details = pageDetails;
            CurrentPageIndex = initialPageIndex;
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