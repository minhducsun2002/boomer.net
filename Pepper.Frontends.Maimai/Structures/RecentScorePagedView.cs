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
            var components = interactionIds[pageIndex]
                .Select((c) => LocalComponent.Button(c.Item1, $"Track {c.Item2}"));
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