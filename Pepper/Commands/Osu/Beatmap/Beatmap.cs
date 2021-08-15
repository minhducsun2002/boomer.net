using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Qmmands;
using Pepper.Services.Osu.API;
using Pepper.Structures.External.Osu;

namespace Pepper.Commands.Osu
{
    public class Beatmap : BeatmapContextCommand
    {
        public Beatmap(APIService service, BeatmapContextProviderService b) : base(service, b) {}

        [Command("map", "beatmap")]
        public async Task<DiscordCommandResult> Exec(
            [Description("Beatmap(set) ID, or an URL.")] IBeatmapOrSetResolvable beatmapResolvable,
            [Flag("/")] bool set = false
        )
        {
            return beatmapResolvable switch
            {
                BeatmapsetResolvable beatmapset => Beatmapset(
                    await APIService.GetBeatmapsetInfo(beatmapset.BeatmapsetId, true)),
                BeatmapResolvable beatmap => await BeatmapSingle(
                    await APIService.GetBeatmapsetInfo(beatmap.BeatmapId, false), beatmap.BeatmapId),
                BeatmapAndSetResolvable beatmapAndSet => await BeatmapSingle(
                    await APIService.GetBeatmapsetInfo(beatmapAndSet.BeatmapsetId, true), beatmapAndSet.BeatmapId),
                BeatmapOrSetResolvable beatmapOrSet =>  set switch
                    {
                        true => Beatmapset(await APIService.GetBeatmapsetInfo(beatmapOrSet.BeatmapOrSetId, true)),
                        false => await BeatmapSingle(await APIService.GetBeatmapsetInfo(beatmapOrSet.BeatmapOrSetId, false), beatmapOrSet.BeatmapOrSetId)
                    },
                _ => throw new ArgumentException("A valid URL is not provided!")
            };
        }

        private DiscordCommandResult Beatmapset(APIBeatmapSet beatmapset) => View(new BeatmapsetPagedView(beatmapset));
        private async Task<DiscordCommandResult> BeatmapSingle(APIBeatmapSet beatmapset, int beatmapId)
        {
            var embed = await BeatmapSingleView.PrepareEmbed(beatmapset, APIService, beatmapId);
            SetBeatmapContext(beatmapId);
            return View(new BeatmapSingleView(beatmapset, APIService, embed, beatmapId));
        }
    }
}