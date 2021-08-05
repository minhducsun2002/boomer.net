using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BAMCIS.ChunkExtensionMethod;
using Disqord;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
using Disqord.Extensions.Interactivity.Menus.Paged;
using osu.Game.Beatmaps;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Pepper.Utilities.Osu;
using Qmmands;
using Pepper.Services.Osu.API;
using Pepper.Structures.External.Osu;

namespace Pepper.Commands.Osu
{
    public class Beatmap : OsuCommand
    {
        public Beatmap(APIService service) : base(service) {}

        [Command("map", "beatmap")]
        public async Task<DiscordCommandResult> Exec(
            [Description("Beatmap(set) ID, or an URL.")] string beatmapResolvable,
            [Flag("/")] bool set = false
        )
        {
            if (URLParser.CheckMapUrl(beatmapResolvable, out _, out var id, out var setId))
            {
                if (id != null) return Beatmapset(await APIService.GetBeatmapsetInfo((int) id, false));
                if (setId != null) return Beatmapset(await APIService.GetBeatmapsetInfo((int) setId, true));
            }

            throw new ArgumentException("A valid URL is not provided!");
        }

        private DiscordCommandResult Beatmapset(APIBeatmapSet beatmapSet) => View(new BeatmapsetPagedView(beatmapSet));
    }
}