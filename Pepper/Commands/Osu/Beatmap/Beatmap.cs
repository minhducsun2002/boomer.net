using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Pepper.Utilities.Osu;
using Qmmands;
using Pepper.Services.Osu.API;

namespace Pepper.Commands.Osu
{
    public class Beatmap : BeatmapContextCommand
    {
        public Beatmap(APIService service, BeatmapContextProviderService b) : base(service, b) {}

        [Command("map", "beatmap")]
        public async Task<DiscordCommandResult> Exec(
            [Description("Beatmap(set) ID, or an URL.")] string beatmapResolvable = "",
            [Flag("/")] bool set = false
        )
        {
            if (string.IsNullOrWhiteSpace(beatmapResolvable))
                beatmapResolvable = GetBeatmapContext()?.ToString() ?? "";
            
            if (URLParser.CheckMapUrl(beatmapResolvable, out _, out var id, out var setId))
            {
                if (id != null) return await BeatmapSingle(await APIService.GetBeatmapsetInfo((int) id, false), (int) id);
                if (setId != null) return Beatmapset(await APIService.GetBeatmapsetInfo((int) setId, true));
            }

            if (int.TryParse(beatmapResolvable, out var targetId))
                return set switch
                {
                    true => Beatmapset(await APIService.GetBeatmapsetInfo(targetId, set)),
                    false => await BeatmapSingle(await APIService.GetBeatmapsetInfo(targetId, set), targetId)
                };

            throw new ArgumentException("A valid URL is not provided!");
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