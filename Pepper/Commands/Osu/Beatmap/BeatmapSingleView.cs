using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Limits = Disqord.Discord.Limits;

namespace Pepper.Commands.Osu
{
    internal class BeatmapPageProvider : PageProvider
    {
        public readonly APIBeatmapSet Beatmapset;
        private readonly APIClient apiService;
        private readonly ModParserService modParserService;
        private readonly Ruleset ruleset;
        private readonly Dictionary<string, LocalEmbed> cachedBeatmapEmbeds = new();

        public BeatmapPageProvider(
            APIBeatmapSet beatmapSet,
            APIClient apiService,
            ModParserService modParserService,
            Ruleset ruleset
        )
        {
            Beatmapset = beatmapSet;
            this.apiService = apiService;
            this.modParserService = modParserService;
            this.ruleset = ruleset;
        }

        public override async ValueTask<Page> GetPageAsync(PagedViewBase view)
        {
            var currentIndex = view.CurrentPageIndex;

            var mods = Array.Empty<Mod>();
            if (view is BeatmapSingleView beatmapSingleView)
            {
                mods = modParserService.ResolveMods(ruleset, beatmapSingleView.CurrentMods);
            }

            mods = mods.OrderBy(m => m.Acronym).ToArray();

            var key = currentIndex + string.Join("", mods.Select(m => m.Acronym));

            if (!cachedBeatmapEmbeds.TryGetValue(key, out var embed))
            {
                embed = cachedBeatmapEmbeds[key] = await PrepareEmbed(Beatmapset, apiService, Beatmapset.Beatmaps[currentIndex].OnlineID, mods);
            }

            return new Page().WithEmbeds(embed);
        }

        public override int PageCount => Beatmapset.Beatmaps.Length;

        private async Task<LocalEmbed> PrepareEmbed(APIBeatmapSet beatmapset, APIClient service, int beatmapId, Mod[]? mods = null)
        {
            mods ??= Array.Empty<Mod>();
            var beatmap = beatmapset.Beatmaps.First(beatmap => beatmap.OnlineID == beatmapId);
            var workingBeatmap = await service.GetBeatmap(beatmapId);
            var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods);
            var pp = new[] { 95, 97, 98, 99, 100 }
                .ToDictionary(
                    acc => acc,
                    acc =>
                    {
                        var pp = workingBeatmap.CalculatePerformance(
                            rulesetOverwrite: ruleset,
                            score: new ScoreInfo { Mods = mods, MaxCombo = difficulty.MaxCombo, Accuracy = (double) acc / 100 }
                        );
                        return pp;
                    }
                );

            return new LocalEmbed
            {
                Title = $"{beatmapset.Artist} - {beatmapset.Title} [{beatmap.DifficultyName}]"
                    + (mods.Length != 0
                        ? "+" + string.Join("", mods.Select(m => m.Acronym))
                        : ""),
                Author = OsuCommand.SerializeAuthorBuilder(beatmapset.Author),
                Url = $"https://osu.ppy.sh/beatmapsets/{beatmapset.OnlineID}#{ruleset.ShortName}/{beatmap.OnlineID}",
                Description = (int) beatmapset.Status < 0
                    ? (beatmapset.Status == BeatmapOnlineStatus.WIP ? "WIP." : $"{beatmapset.Status}.")
                      + $" Last updated **{OsuCommand.SerializeTimestamp(beatmapset.LastUpdated)}**."
                    : $"Ranked **{OsuCommand.SerializeTimestamp(beatmapset.Ranked!.Value)}**.",
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Difficulty",
                        Value = OsuCommand.SerializeBeatmapStats(
                            workingBeatmap.BeatmapInfo, mods, difficulty,
                            workingBeatmap.Beatmap.ControlPointInfo, false
                        )
                    },
                    new()
                    {
                        Name = "PP if FC" + (mods.Length != 0 ? " (with mods applied)" : ""),
                        Value = string.Join(
                            '\n',
                            pp
                                .Chunk(3)
                                .Select(
                                    line =>
                                        string.Join(
                                            " • ",
                                            line.Select(p => $"**{p.Key}**% : **{p.Value:F2}**pp")
                                        )
                                )
                        )
                    }
                },
                Footer = new LocalEmbedFooter().WithText(ruleset.RulesetInfo.Name),
                ThumbnailUrl = $"https://b.ppy.sh/thumb/{beatmapset.OnlineID}l.jpg"
            };
        }
    }

    internal class BeatmapSingleView : PagedViewBase
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
                        button.Style = CurrentMods.Contains(button.Label)
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
            return beatmapset.Beatmaps.Take(Limits.ApplicationCommands.MaxOptionAmount)
                .OrderBy(beatmap => beatmap.StarRating)
                .Select(beatmap => new LocalSelectionComponentOption
                {
                    Label = beatmap.DifficultyName.Length < Limits.Components.Selection.Option.MaxLabelLength
                        ? beatmap.DifficultyName
                        : beatmap.DifficultyName[..(Limits.Components.Selection.Option.MaxLabelLength - 3)] + "...",
                    Description = OsuCommand.SerializeBeatmapStats(beatmapset, beatmap, false, false),
                    Value = $"{beatmap.OnlineID}",
                    IsDefault = CurrentPageIndex == beatmapIdToIndex[beatmap.OnlineID]
                })
                .ToList();
        }

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