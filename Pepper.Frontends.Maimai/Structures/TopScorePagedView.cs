using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using PagedView = Pepper.Commons.Structures.Views.PagedView;

namespace Pepper.Frontends.Maimai.Structures
{
    public class TopScorePagedView : PagedView
    {
        private readonly TopScorePageProvider pageProvider;
        private bool HasOld => pageProvider.OldPages.Length != 0;
        private bool HasNew => pageProvider.NewPages.Length != 0;
        private bool IsOld => CurrentPageIndex >= pageProvider.OldIndex;
        private bool IsNew => !IsOld;

        private readonly ButtonViewComponent? switchButton;

        // TODO: Split this down to common paged score class
        public TopScorePagedView(TopScorePageProvider pageProvider) : base(pageProvider)
        {
            this.pageProvider = pageProvider;
            if (HasNew == HasOld)
            {
                AddComponent(switchButton = new ButtonViewComponent(e =>
                {
                    CurrentPageIndex = IsNew ? pageProvider.OldIndex : default;
                    e.Button.Label = $"Switch to {(IsNew ? "old" : "new")}";
                    return ValueTask.CompletedTask;
                })
                {
                    Label = $"Switch to {(IsNew ? "old" : "new")}",
                    Style = LocalButtonComponentStyle.Secondary
                });
            }
        }

        public override ValueTask UpdateAsync()
        {
            if (switchButton != null)
            {
                switchButton.Label = $"Switch to {(IsNew ? "old" : "new")}";
            }
            return base.UpdateAsync();
        }

        public override void FormatLocalMessage(LocalMessageBase message)
        {
            base.FormatLocalMessage(message);
            var sortedButtonIds = new Dictionary<string, List<int>>();
            var rec = pageProvider.GetPage(CurrentPageIndex);
            if (rec == null)
            {
                return;
            }

            var list = rec.Value.Item2;
            for (var i = 0; i < list.Count; i++)
            {
                if (!sortedButtonIds.TryGetValue(list[i].Item1, out var ids))
                {
                    ids = new List<int>();
                }

                ids.Add(list[i].Item2);
                sortedButtonIds[list[i].Item1] = ids;
            }

            var components = sortedButtonIds
                .Take(5 - 1)
                .Select(c =>
                    LocalComponent.Button(c.Key, string.Join(" / ", c.Value))
                        .WithStyle(LocalButtonComponentStyle.Secondary)
                )
                .Prepend(LocalComponent.Button(Guid.Empty.ToString(), "Check your score :").WithIsDisabled());

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