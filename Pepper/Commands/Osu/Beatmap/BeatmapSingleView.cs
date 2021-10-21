using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Pepper.Services.Osu;
using Pepper.Services.Osu.API;
using Pepper.Structures.External.Osu;

namespace Pepper.Commands.Osu
{
    internal class BeatmapPageProvider : PageProvider
    {
        public readonly APIBeatmapSet Beatmapset;
        private readonly APIService apiService;
        private readonly Dictionary<int, LocalEmbed> beatmapEmbeds = new();
        public BeatmapPageProvider(APIBeatmapSet beatmapSet, APIService apiService)
        {
            Beatmapset = beatmapSet;
            this.apiService = apiService;
        }

        public override async ValueTask<Page> GetPageAsync(PagedViewBase view)
        {
            var currentIndex = view.CurrentPageIndex;
            if (!beatmapEmbeds.TryGetValue(currentIndex, out var embed))
            {
                embed = beatmapEmbeds[currentIndex] = await PrepareEmbed(Beatmapset, apiService, Beatmapset.Beatmaps[currentIndex].OnlineBeatmapID);
            }

            return new Page().WithEmbeds(embed);
        }

        public override int PageCount => Beatmapset.Beatmaps.Count;

        private const double AccStart = 93, AccEnd = 100, AccStep = 0.5;
        private static async Task<LocalEmbed> PrepareEmbed(APIBeatmapSet beatmapset, APIService service, int beatmapId, Mod[]? mods = null)
        {
            var beatmap = beatmapset.Beatmaps.First(beatmap => beatmap.OnlineBeatmapID == beatmapId);
            var ruleset = RulesetTypeParser.SupportedRulesets[beatmap.Ruleset];
            var workingBeatmap = await service.GetBeatmap(beatmapId);
            var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods ?? Array.Empty<Mod>());

            var data = new List<(double, double)>();
            for (var current = AccStart; current <= AccEnd; current += AccStep)
            {
                var fcScore = new ScoreInfo { MaxCombo = difficulty.MaxCombo, Accuracy = current / 100 };
                fcScore.SetCountMiss(0);
                var pp = OsuCommand.GetPerformanceCalculator(beatmap.Ruleset, difficulty, fcScore).Calculate();
                data.Add((current, pp));
            }

            var chart = new QuickChart.Chart
            {
                Height = 300,
                Width = 500,
                Config = $@"{{
                    type: 'line',
                    data: {{
                        labels: [{string.Join(", ", data.Select(pair => $"\"{pair.Item1}%\""))}],
                        datasets: [{{
                            label: '{beatmapset.Artist} - {beatmapset.Title}',
                            data: [{string.Join(", ", data.Select(pair => $"{pair.Item2:F2}"))}],
                            borderColor: '#42adf5',
                            backgroundColor: 'black'
                        }}],
                        options: {{ scales: {{ y: {{ min: {data.Min(pair => pair.Item2)} }} }} }}
                    }}      
                }}",
                BackgroundColor = "white"
            };

            return new LocalEmbed
            {
                Title = $"{beatmapset.Artist} - {beatmapset.Title} [{beatmap.Version}]",
                Author = OsuCommand.SerializeAuthorBuilder(beatmapset.Author),
                Url = $"https://osu.ppy.sh/beatmapsets/{beatmapset.OnlineBeatmapSetID}#{ruleset.ShortName}/{beatmap.OnlineBeatmapID}",
                Description = (int) beatmapset.Status < 0
                    ? (beatmapset.Status == BeatmapSetOnlineStatus.WIP ? "WIP." : $"{beatmapset.Status}.")
                      + $" Last updated **{OsuCommand.SerializeTimestamp(beatmapset.LastUpdated)}**."
                    : $"Ranked **{OsuCommand.SerializeTimestamp(beatmapset.Ranked!.Value)}**.",
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Difficulty",
                        Value = OsuCommand.SerializeBeatmapStats(workingBeatmap.BeatmapInfo, difficulty, workingBeatmap.Beatmap.ControlPointInfo, false)
                    }
                },
                Footer = new LocalEmbedFooter().WithText(ruleset.RulesetInfo.Name),
                ImageUrl = chart.GetUrl() + "&version=3"
            };
        }
    }

    internal class BeatmapSingleView : PagedViewBase
    {
        private readonly APIBeatmapSet beatmapset;
        private readonly Dictionary<int, int> beatmapIdToIndex;
        public BeatmapSingleView(BeatmapPageProvider pageProvider, int initialBeatmapId) : base(pageProvider)
        {
            beatmapset = pageProvider.Beatmapset;
            beatmapIdToIndex = beatmapset.Beatmaps.Select((beatmap, index) => (beatmap.OnlineBeatmapID, index))
                .ToDictionary(pair => pair.OnlineBeatmapID, pair => pair.index);

            CurrentPageIndex = beatmapIdToIndex[initialBeatmapId];
            var select = new SelectionViewComponent(async e =>
            {
                CurrentPageIndex = beatmapIdToIndex[int.Parse(e.SelectedOptions[0].Value)];
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
        }

        private List<LocalSelectionComponentOption> GetCurrentOptionList()
        {
            return beatmapset.Beatmaps.Take(LocalSelectionComponent.MaxOptionsAmount)
                .OrderBy(beatmap => beatmap.StarDifficulty)
                .Select(beatmap => new LocalSelectionComponentOption
                {
                    Label = beatmap.Version.Length < LocalSelectionComponentOption.MaxLabelLength
                        ? beatmap.Version
                        : beatmap.Version[..22] + "...",
                    Description = OsuCommand.SerializeBeatmapStats(beatmapset, beatmap, false, false),
                    Value = $"{beatmap.OnlineBeatmapID}",
                    IsDefault = CurrentPageIndex == beatmapIdToIndex[beatmap.OnlineBeatmapID]
                })
                .ToList();
        }
    }
}