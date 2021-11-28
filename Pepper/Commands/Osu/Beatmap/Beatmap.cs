using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.Configuration;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Pepper.Structures.External.Osu;
using Qmmands;

namespace Pepper.Commands.Osu
{
    public class Beatmap : BeatmapContextCommand
    {
        private readonly IConfiguration configuration;

        public Beatmap(IAPIClient service, BeatmapContextProviderService b, IConfiguration configuration) : base(service, b)
        {
            this.configuration = configuration;
        }

        [Command("map", "beatmap")]
        [Description("View information about a beatmap(set).")]
        public async Task<DiscordCommandResult> Exec(
            [Description("Beatmap(set) ID, or an URL.")] IBeatmapOrSetResolvable beatmapResolvable,
            [Description("In case a numeric ID is passed, whether this is a set ID.")][Flag("/")] bool set = false
        )
        {
            return beatmapResolvable switch
            {
                BeatmapsetResolvable beatmapset => Beatmapset(
                    await APIService.GetBeatmapsetInfo(beatmapset.BeatmapsetId, true)),
                BeatmapResolvable beatmap => BeatmapSingle(
                    await APIService.GetBeatmapsetInfo(beatmap.BeatmapId, false), beatmap.BeatmapId),
                BeatmapAndSetResolvable beatmapAndSet => BeatmapSingle(
                    await APIService.GetBeatmapsetInfo(beatmapAndSet.BeatmapsetId, true), beatmapAndSet.BeatmapId),
                BeatmapOrSetResolvable beatmapOrSet => set switch
                {
                    true => Beatmapset(await APIService.GetBeatmapsetInfo(beatmapOrSet.BeatmapOrSetId, true)),
                    false => BeatmapSingle(await APIService.GetBeatmapsetInfo(beatmapOrSet.BeatmapOrSetId, false), beatmapOrSet.BeatmapOrSetId)
                },
                _ => throw new ArgumentException("A valid URL is not provided!")
            };
        }

        private DiscordCommandResult Beatmapset(APIBeatmapSet beatmapset) => View(new BeatmapsetPagedView(beatmapset));
        private DiscordCommandResult BeatmapSingle(APIBeatmapSet beatmapset, int beatmapId)
        {
            SetBeatmapContext(beatmapId);
            return View(new BeatmapSingleView(new BeatmapPageProvider(beatmapset, APIService, configuration), beatmapId));
        }
    }
}