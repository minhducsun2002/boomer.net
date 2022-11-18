using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Rest;
using Pepper.Structures.External.Osu;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;
using Limits = Disqord.Discord.Limits;
using PagedViewBase = Pepper.Commons.Structures.Views.PagedViewBase;

namespace Pepper.Commands.Osu
{
    internal class BeatmapSingleView : Commons.Structures.Views.PagedViewBase
    {
        private readonly APIBeatmapSet beatmapset;
        private readonly Dictionary<int, int> beatmapIdToIndex;
        public ImmutableHashSet<string> CurrentMods = ImmutableHashSet<string>.Empty;
        private static readonly HashSet<string> AllowedMods = new(new[] { "DT", "HR", "HD", "FL", "EZ" });

        public BeatmapSingleView(BeatmapPageProvider pageProvider, int initialBeatmapId) : base(pageProvider)
        {
            beatmapset = pageProvider.Beatmapset;
            beatmapIdToIndex = beatmapset.Beatmaps.Select((beatmap, index) => (beatmap.OnlineID, index))
                .ToDictionary(pair => pair.OnlineID, pair => pair.index);

            CurrentPageIndex = beatmapIdToIndex[initialBeatmapId];
            var select = new SelectionViewComponent(async e =>
            {
                CurrentPageIndex = beatmapIdToIndex[int.Parse(e.SelectedOptions[0].Value.Value)];
                await e.Interaction.Response().DeferAsync();
                e.Selection.Options = GetCurrentOptionList();
            })
            {
                MaximumSelectedOptions = 1,
                MinimumSelectedOptions = 1,
                Placeholder = "Jump...",
                Options = GetCurrentOptionList()
            };

            AddComponent(select);
            foreach (var mod in AllowedMods)
            {
                var button = new ButtonViewComponent(async e =>
                {
                    await e.Interaction.Response().DeferAsync();
                    CurrentMods = CurrentMods.Contains(mod)
                        ? CurrentMods.Remove(mod)
                        : CurrentMods.Add(mod);

                    // HR and EZ is exclusive
                    switch (mod)
                    {
                        case "HR":
                            if (CurrentMods.Contains(mod))
                            {
                                CurrentMods = CurrentMods.Remove("EZ");
                            }
                            break;
                        case "EZ":
                            if (CurrentMods.Contains(mod))
                            {
                                CurrentMods = CurrentMods.Remove("HR");
                            }
                            break;
                    }

                    foreach (var button in EnumerateComponents().OfType<ButtonViewComponent>())
                    {
                        button.Style = CurrentMods.Contains(button.Label ?? "")
                            ? LocalButtonComponentStyle.Success
                            : LocalButtonComponentStyle.Secondary;
                    }

                    ReportPageChanges();
                })
                {
                    Label = mod,
                    Style = LocalButtonComponentStyle.Secondary
                };
                AddComponent(button);
            }
        }

        private List<LocalSelectionComponentOption> GetCurrentOptionList()
        {
            return beatmapset.Beatmaps.Take(Limits.Component.Selection.MaxOptionAmount)
                .OrderBy(beatmap => beatmap.StarRating)
                .Select(beatmap => new LocalSelectionComponentOption
                {
                    Label = beatmap.DifficultyName.Length < Limits.Component.Selection.Option.MaxLabelLength
                        ? beatmap.DifficultyName
                        : beatmap.DifficultyName[..(Limits.Component.Selection.Option.MaxLabelLength - 3)] + "...",
                    Description = new BeatmapStatsSerializer(beatmap)
                        .Serialize(formatted: false, serializationOptions: StatFilter.StarRating | StatFilter.Statistics),
                    Value = $"{beatmap.OnlineID}",
                    IsDefault = CurrentPageIndex == beatmapIdToIndex[beatmap.OnlineID]
                })
                .ToList();
        }
    }
}