using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Disqord.Rest;
using Microsoft.Extensions.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Structures.External.Osu;

namespace Pepper.Commands.Osu
{
    internal class BeatmapPageProvider : PageProvider
    {
        public readonly APIBeatmapSet Beatmapset;
        private readonly APIClient apiService;
        private readonly string imageHost;
        private readonly Dictionary<int, LocalEmbed> beatmapEmbeds = new();
        public BeatmapPageProvider(APIBeatmapSet beatmapSet, APIClient apiService, IConfiguration configuration)
        {
            Beatmapset = beatmapSet;
            this.apiService = apiService;
            imageHost = configuration.GetSection("osu:pp_chart_host").Get<string[]>()[0];
        }

        public override async ValueTask<Page> GetPageAsync(PagedViewBase view)
        {
            var currentIndex = view.CurrentPageIndex;
            if (!beatmapEmbeds.TryGetValue(currentIndex, out var embed))
            {
                embed = beatmapEmbeds[currentIndex] = await PrepareEmbed(Beatmapset, apiService, Beatmapset.Beatmaps[currentIndex].OnlineID);
            }

            return new Page().WithEmbeds(embed);
        }

        public override int PageCount => Beatmapset.Beatmaps.Length;

        private async Task<LocalEmbed> PrepareEmbed(APIBeatmapSet beatmapset, APIClient service, int beatmapId, Mod[]? mods = null)
        {
            var beatmap = beatmapset.Beatmaps.First(beatmap => beatmap.OnlineID == beatmapId);
            var ruleset = RulesetTypeParser.SupportedRulesets[beatmap.RulesetID];
            var workingBeatmap = await service.GetBeatmap(beatmapId);
            var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods ?? Array.Empty<Mod>());

            return new LocalEmbed
            {
                Title = $"{beatmapset.Artist} - {beatmapset.Title} [{beatmap.DifficultyName}]",
                Author = OsuCommand.SerializeAuthorBuilder(beatmapset.Author!),
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
                        Value = OsuCommand.SerializeBeatmapStats(workingBeatmap.BeatmapInfo, difficulty, workingBeatmap.Beatmap.ControlPointInfo, false)
                    }
                },
                Footer = new LocalEmbedFooter().WithText(ruleset.RulesetInfo.Name),
                ImageUrl = $"https://{imageHost}/performance/chart/{beatmap.OnlineID}.png"
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
            beatmapIdToIndex = beatmapset.Beatmaps.Select((beatmap, index) => (beatmap.OnlineID, index))
                .ToDictionary(pair => pair.OnlineID, pair => pair.index);

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
                .OrderBy(beatmap => beatmap.StarRating)
                .Select(beatmap => new LocalSelectionComponentOption
                {
                    Label = beatmap.DifficultyName.Length < LocalSelectionComponentOption.MaxLabelLength
                        ? beatmap.DifficultyName
                        : beatmap.DifficultyName[..22] + "...",
                    Description = OsuCommand.SerializeBeatmapStats(beatmapset, beatmap, false, false),
                    Value = $"{beatmap.OnlineID}",
                    IsDefault = CurrentPageIndex == beatmapIdToIndex[beatmap.OnlineID]
                })
                .ToList();
        }
    }
}