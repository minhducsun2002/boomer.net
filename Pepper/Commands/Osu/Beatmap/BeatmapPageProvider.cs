using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Disqord;
using Disqord.Extensions.Interactivity.Menus.Paged;
using Humanizer;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Structures.External.Osu;

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

        public override async ValueTask<Page?> GetPageAsync(PagedViewBase view)
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
            var difficulty = workingBeatmap.CalculateDifficulty(ruleset.RulesetInfo.OnlineID, true, mods);
            var synthesizer = new HitStatisticsSynthesizer(workingBeatmap.Beatmap.HitObjects.Count);
            var pp = new[] { 95, 97, 98, 99, 100 }
                .ToDictionary(
                    acc => acc,
                    acc =>
                    {
                        var stats = synthesizer.Synthesize(ruleset, (double) acc / 100);
                        var pp = workingBeatmap.CalculatePerformance(
                            rulesetOverwrite: ruleset,
                            score: new ScoreInfo { Mods = mods, MaxCombo = difficulty.MaxCombo, Accuracy = (double) acc / 100, Statistics = stats }
                        );
                        return pp;
                    }
                );

            var hitTypes = new Dictionary<string, int>();
            foreach (var hitobject in workingBeatmap.Beatmap.HitObjects)
            {
                var type = hitobject.GetType().Name;
                if (type.StartsWith("Convert"))
                {
                    type = type["Convert".Length..];
                }

                type = type.ToLowerInvariant();

                hitTypes[type] = hitTypes.TryGetValue(type, out var value) ? value + 1 : 1;
            }

            return new LocalEmbed
            {
                Title = $"{beatmapset.Artist} - {beatmapset.Title} [{beatmap.DifficultyName}]"
                        + (mods.Length != 0
                            ? "+" + string.Join("", mods.Select(m => m.Acronym))
                            : ""),
                Author = OsuCommand.SerializeAuthorBuilder(beatmapset.Author),
                Url = $"https://osu.ppy.sh/beatmapsets/{beatmapset.OnlineID}#{ruleset.ShortName}/{beatmap.OnlineID}",
                Description = (int) beatmapset.Status <= 0
                    ? (beatmapset.Status == BeatmapOnlineStatus.WIP ? "WIP." : $"{beatmapset.Status}.")
                      + $" Last updated **{OsuCommand.SerializeTimestamp(beatmapset.LastUpdated)}**."
                    : $"Ranked **{OsuCommand.SerializeTimestamp(beatmapset.Ranked!.Value)}**.",
                Fields = new List<LocalEmbedField>
                {
                    new()
                    {
                        Name = "Difficulty",
                        Value = new BeatmapStatsSerializer(workingBeatmap.BeatmapInfo)
                                {
                                    Mods = mods,
                                    ControlPointInfo = workingBeatmap.Beatmap.ControlPointInfo,
                                    DifficultyOverwrite = difficulty
                                }.Serialize(
                                    formatted: true,
                                    serializationOptions: StatFilter.Statistics |
                                                          StatFilter.BPM |
                                                          StatFilter.StarRating |
                                                          (false ? StatFilter.Length : 0)
                                )
                                + $"\n**{difficulty.MaxCombo}**x"
                                + string.Join(
                                    "",
                                    hitTypes
                                        .OrderBy(p => p.Key)
                                        .Select(pair => $" • {pair.Value} {(pair.Value > 1 ? pair.Key.Pluralize() : pair.Key)}")
                                )
                                + " • "
                                + new BeatmapStatsSerializer(workingBeatmap.BeatmapInfo).Serialize(
                                    formatted: true,
                                    serializationOptions: StatFilter.Length
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
}