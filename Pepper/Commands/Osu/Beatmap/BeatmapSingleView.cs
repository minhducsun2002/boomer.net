using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using Pepper.Services.Osu;
using Pepper.Services.Osu.API;
using Pepper.Structures.External.Osu;

namespace Pepper.Commands.Osu
{
    internal class BeatmapSingleView : ViewBase
    {
        private readonly Dictionary<int, LocalEmbed> beatmapEmbeds = new();
        private int currentBeatmapId;
        private readonly APIBeatmapSet beatmapset;
        
        public BeatmapSingleView(APIBeatmapSet beatmapset, APIService service, LocalEmbed initialEmbed, int initialBeatmapId) 
            : base(new LocalMessage { Embeds = new List<LocalEmbed> { initialEmbed } })
        {
            beatmapEmbeds[initialBeatmapId] = initialEmbed;
            currentBeatmapId = initialBeatmapId;
            this.beatmapset = beatmapset;

            var select = new SelectionViewComponent(async e =>
            {
                var beatmapId = int.Parse(e.SelectedOptions[0].Value);
                if (!beatmapEmbeds.TryGetValue(beatmapId, out var embed))
                    embed = beatmapEmbeds[beatmapId] = await PrepareEmbed(beatmapset, service, beatmapId);

                currentBeatmapId = beatmapId;

                    TemplateMessage.Embeds = new List<LocalEmbed> {embed};
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
                    Label = $"{beatmap.Version}",
                    Description = OsuCommand.SerializeBeatmapStats(beatmapset, beatmap, false, false),
                    Value = $"{beatmap.OnlineBeatmapID}",
                    IsDefault = currentBeatmapId == beatmap.OnlineBeatmapID
                })
                .ToList();
        }

        public static async Task<LocalEmbed> PrepareEmbed(APIBeatmapSet beatmapset, APIService service, int beatmapId, Mod[]? mods = null)
        {
            var beatmap = beatmapset.Beatmaps.First(beatmap => beatmap.OnlineBeatmapID == beatmapId);
            var ruleset = RulesetTypeParser.SupportedRulesets[beatmap.Ruleset];
            var workingBeatmap = await service.GetBeatmap(beatmapId);
            var difficulty = ruleset.CreateDifficultyCalculator(workingBeatmap).Calculate(mods ?? Array.Empty<Mod>());
            
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
                    },
                    new()
                    {
                        Name = "Game mode",
                        Value = ruleset.RulesetInfo.Name,
                        IsInline = true
                    }
                }
            };
        }
    }
}