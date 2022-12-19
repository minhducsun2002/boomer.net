using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.Maimai.Structures
{
    public partial class RecentScorePagedView : PagedView
    {
        private readonly List<List<(string, int)>> interactionIds;
        private RecentScorePagedView(
            PageProvider pageProvider,
            List<List<(string, int)>> interactionIds,
            Action<LocalMessageBase>? messageTemplate = null
        ) : base(pageProvider, messageTemplate)
        {
            this.interactionIds = interactionIds;
        }

        public override void FormatLocalMessage(LocalMessageBase message)
        {
            base.FormatLocalMessage(message);
            var pageIndex = CurrentPageIndex;
            var sortedButtonIds = new Dictionary<string, List<int>>();
            foreach (var (buttonId, trackNumber) in interactionIds[pageIndex])
            {
                if (!sortedButtonIds.TryGetValue(buttonId, out var ids))
                {
                    ids = new List<int>();
                }

                ids.Add(trackNumber);
                sortedButtonIds[buttonId] = ids;
            }
            var components = sortedButtonIds
                .Select(c => LocalComponent.Button(c.Key, $"Track {string.Join(" / ", c.Value)}"));
            if (message.Components.HasValue)
            {
                // ReSharper disable once CoVariantArrayConversion
                message.Components.Value.Add(LocalComponent.Row(components.ToArray()));
            }
            else
            {
                // ReSharper disable once CoVariantArrayConversion
                message.Components = new[] { LocalComponent.Row(components.ToArray()) };
            }
        }
    }
}