using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Beatmaps;
using Pepper.Services.Osu.API;
using Pepper.Structures.External.Osu;

namespace Pepper.Commands.Osu
{
    internal class BeatmapsetPagedView : PagedView
    {
        private readonly Dictionary<int, ButtonViewComponent> pageJumps;
        private const int CustomRow = 1;

        public BeatmapsetPagedView(APIBeatmapSet beatmapset) : base(new ListPageProvider(PreparePages(beatmapset, out var jumps)))
        {
            RemoveComponent(StopButton);
            RemoveComponent(FirstPageButton);
            RemoveComponent(LastPageButton);

            PreviousPageButton.Label = "Previous page";
            PreviousPageButton.Emoji = null;
            
            NextPageButton.Label = "Next page";
            NextPageButton.Emoji = null;

            pageJumps = new Dictionary<int, ButtonViewComponent>();
            foreach (var (text, index) in jumps)
                AddComponent(pageJumps[index] = new ButtonViewComponent(e =>
                {
                    CurrentPageIndex = index;
                    UpdateButtonStates();
                    return default;
                })
                {
                    Label = text,
                    Row = CustomRow,
                });
            
        }

        private void UpdateButtonStates()
        {
            foreach (var viewComponent in EnumerateComponents())
                // if (viewComponent is ButtonViewComponent button && button.Row == CustomRow)
                if (viewComponent is ButtonViewComponent {Row: CustomRow} button)
                    button.IsDisabled = false;
                

            var index = 0;
            foreach (var (first, _) in pageJumps) if (first <= CurrentPageIndex) index = Math.Max(index, first);
            pageJumps[index].IsDisabled = true;
        }

        protected override ValueTask OnNextPageButtonAsync(ButtonEventArgs e)
        {
            UpdateButtonStates();
            return base.OnNextPageButtonAsync(e);
        }
    
        protected override ValueTask OnLastPageButtonAsync(ButtonEventArgs e)
        {
            UpdateButtonStates();
            return base.OnLastPageButtonAsync(e);
        }

        private const int MaxDiffPerPage = 7;
        private static IEnumerable<Page> PreparePages(APIBeatmapSet beatmapset, out Dictionary<string, int> jumps)
        {
            var embeds = beatmapset.Beatmaps
                .Concat(beatmapset.Converts)
                .GroupBy(beatmap => beatmap.Ruleset)
                .OrderBy(group => group.Key)
                .Where(group => group.Any())
                .SelectMany(grouping =>
                {
                    var chunked = grouping.OrderBy(map => map.StarDifficulty)
                        .Chunk(MaxDiffPerPage).ToList();
                    var index = 1;
                    var modeEmbeds = chunked.Select(mapChunk => new LocalEmbed
                        {
                            Title = $"{beatmapset.Artist} - {beatmapset.Title}",
                            Author = OsuCommand.SerializeAuthorBuilder(beatmapset.Author),
                            Url = $"https://osu.ppy.sh/beatmapsets/{beatmapset.OnlineBeatmapSetID}",
                            Description = (int) beatmapset.Status < 0
                                ? (beatmapset.Status == BeatmapSetOnlineStatus.WIP ? "WIP." : $"{beatmapset.Status}.")
                                  + $" Last updated **{OsuCommand.SerializeTimestamp(beatmapset.LastUpdated)}**."
                                : $"Ranked **{OsuCommand.SerializeTimestamp(beatmapset.Ranked!.Value)}**.",
                            ImageUrl = beatmapset.Covers.Cover,
                            Fields = mapChunk.Select(map => new LocalEmbedField
                            {
                                Name = $"[{map.StarDifficulty:F2}⭐] {map.Version}",
                                Value = (map.MaxCombo != null ? $"**{map.MaxCombo}**x" : "")
                                        + $" • `CS`**{map.CircleSize}** `AR`**{map.ApproachRate}** `OD`**{map.OverallDifficulty}** `HP`**{map.DrainRate}**"
                                        + $" • **{beatmapset.Bpm:0.}** BPM"
                                        + $@" • :clock3: {
                                            (map.TotalLengthInSeconds / 60).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')
                                        }:{(map.TotalLengthInSeconds % 60).ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}"
                            }).ToList(),
                            Footer = new LocalEmbedFooter
                            {
                                Text = $"Mode : {RulesetTypeParser.SupportedRulesets[grouping.Key].RulesetInfo.Name} | Page {index++}/{chunked.Count}"
                            }
                        })
                        .Select(embed => (grouping.Key, embed));
                    return modeEmbeds;
                })
                .ToList();

            var outputJumps = new Dictionary<string, int>();
            for (var i = 0; i < embeds.Count; i++)
            {
                var (rulesetId, _) = embeds[i];
                var ruleset = RulesetTypeParser.SupportedRulesets[rulesetId];
                outputJumps[ruleset.RulesetInfo.Name] = i;
            }
            
            jumps = outputJumps;
            return embeds.Select(embed => new Page().AddEmbed(embed.embed));
        }
    }
}