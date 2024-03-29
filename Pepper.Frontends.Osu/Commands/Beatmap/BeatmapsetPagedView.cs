using Disqord;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Beatmaps;
using Pepper.Frontends.Osu.Structures;
using Pepper.Frontends.Osu.Structures.TypeParsers;
using APIBeatmapSet = Pepper.Commons.Osu.API.APIBeatmapSet;

namespace Pepper.Frontends.Osu.Commands
{
    internal class BeatmapsetPagedView : Commons.Structures.Views.PagedView
    {
        private readonly Dictionary<int, ButtonViewComponent> pageJumps = new();
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

            foreach (var (text, index) in jumps)
            {
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

            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            foreach (var viewComponent in EnumerateComponents())
            {
                // if (viewComponent is ButtonViewComponent button && button.Row == CustomRow)
                if (viewComponent is ButtonViewComponent { Row: CustomRow } button)
                {
                    button.IsDisabled = false;
                }
            }

            var index = 0;
            foreach (var (first, _) in pageJumps)
            {
                if (first <= CurrentPageIndex)
                {
                    index = Math.Max(index, first);
                }
            }

            pageJumps[index].IsDisabled = true;
        }

        protected override void ApplyPageIndex(Page page) { }

        protected override ValueTask OnNextPageButton(ButtonEventArgs e)
        {
            var result = base.OnNextPageButton(e);
            UpdateButtonStates();
            return result;
        }

        protected override ValueTask OnLastPageButton(ButtonEventArgs e)
        {
            var result = base.OnLastPageButton(e);
            UpdateButtonStates();
            return result;
        }

        private const int MaxDiffPerPage = 7;
        private static IEnumerable<Page> PreparePages(APIBeatmapSet beatmapset, out Dictionary<string, int> jumps)
        {
            var embeds = beatmapset.Beatmaps
                .Concat(beatmapset.Converts)
                .GroupBy(beatmap => beatmap.RulesetID)
                .OrderBy(group => group.Key)
                .Where(group => group.Any())
                .SelectMany(grouping =>
                {
                    var chunked = grouping.OrderBy(map => map.StarRating)
                        .Chunk(MaxDiffPerPage).ToList();
                    var index = 1;
                    var modeEmbeds = chunked.Select(mapChunk => new LocalEmbed
                    {
                        Title = $"{beatmapset.Artist} - {beatmapset.Title}",
                        Author = OsuCommand.SerializeAuthorBuilder(beatmapset.Author),
                        Url = $"https://osu.ppy.sh/beatmapsets/{beatmapset.OnlineID}",
                        Description = (int) beatmapset.Status < 0
                                ? (beatmapset.Status == BeatmapOnlineStatus.WIP ? "WIP." : $"{beatmapset.Status}.")
                                  + $" Last updated **{OsuCommand.SerializeTimestamp(beatmapset.LastUpdated)}**."
                                : $"Ranked **{OsuCommand.SerializeTimestamp(beatmapset.Ranked!.Value)}**.",
                        ImageUrl = beatmapset.Covers.Cover,
                        Fields = mapChunk.Select(map => new LocalEmbedField
                        {
                            Name = $"[{map.StarRating:F2}⭐] {map.DifficultyName}",
                            Value = new BeatmapStatsSerializer(map)
                                .Serialize(
                                    formatted: true,
                                    serializationOptions: StatFilter.Combo | StatFilter.Statistics | StatFilter.BPM | StatFilter.Length
                                )
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
                outputJumps.TryAdd(RulesetTypeParser.SupportedRulesets[rulesetId].RulesetInfo.Name, i);
            }

            jumps = outputJumps;
            return embeds.Select(embed => new Page().AddEmbed(embed.embed));
        }
    }
}