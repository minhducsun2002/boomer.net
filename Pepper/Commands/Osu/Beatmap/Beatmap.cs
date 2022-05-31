using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Extensions.Interactivity.Menus;
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
        private readonly ModParserService modParserService;
        public Beatmap(
            APIClientStore apiClientStore,
            BeatmapContextProviderService b,
            ModParserService modParserService
        ) : base(apiClientStore, b)
        {
            this.modParserService = modParserService;
        }

        [Command("map", "beatmap")]
        [Description("View information about a beatmap(set).")]
        public async Task<DiscordCommandResult> Exec(
            [Description("Beatmap(set) ID, or an URL.")] IBeatmapOrSetResolvable beatmapResolvable,
            [Description("In case a numeric ID is passed, whether this is a set ID.")][Flag("/")] bool set = false
        )
        {
            var apiService = APIClientStore.GetClient(GameServer.Osu);
            return beatmapResolvable switch
            {
                BeatmapsetResolvable beatmapset => Beatmapset(
                    await apiService.GetBeatmapsetInfo(beatmapset.BeatmapsetId, true)),
                BeatmapResolvable beatmap => BeatmapSingle(
                    await apiService.GetBeatmapsetInfo(beatmap.BeatmapId, false), beatmap.BeatmapId),
                BeatmapAndSetResolvable beatmapAndSet => BeatmapSingle(
                    await apiService.GetBeatmapsetInfo(beatmapAndSet.BeatmapsetId, true), beatmapAndSet.BeatmapId),
                BeatmapOrSetResolvable beatmapOrSet => set switch
                {
                    true => Beatmapset(await apiService.GetBeatmapsetInfo(beatmapOrSet.BeatmapOrSetId, true)),
                    false => BeatmapSingle(await apiService.GetBeatmapsetInfo(beatmapOrSet.BeatmapOrSetId, false), beatmapOrSet.BeatmapOrSetId)
                },
                _ => throw new ArgumentException("A valid URL is not provided!")
            };
        }

        private DiscordCommandResult Beatmapset(APIBeatmapSet beatmapset)
            => Menu(new DefaultMenu(new BeatmapsetPagedView(beatmapset)));

        private DiscordCommandResult BeatmapSingle(APIBeatmapSet beatmapset, int beatmapId)
        {
            SetBeatmapContext(beatmapId);

            var pageProvider = new BeatmapPageProvider(
                beatmapset,
                APIClientStore.GetClient(GameServer.Osu),
                modParserService,
                RulesetTypeParser.SupportedRulesets[beatmapset.Beatmaps.First(b => b.OnlineID == beatmapId).RulesetID]
            );
            return Menu(
                new DefaultMenu(
                    new BeatmapSingleView(pageProvider, beatmapId)
                )
            );
        }
    }
}