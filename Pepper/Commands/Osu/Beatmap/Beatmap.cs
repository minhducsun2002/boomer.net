using System;
using System.Linq;
using System.Threading.Tasks;
using Disqord.Bot;
using Disqord.Bot.Commands;
using Disqord.Extensions.Interactivity.Menus;
using Pepper.Commons.Osu;
using Pepper.Commons.Osu.API;
using Pepper.Services.Osu;
using Pepper.Structures;
using Pepper.Structures.External.Osu;
using Qmmands;
using Qmmands.Text;

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

        [TextCommand("map", "beatmap")]
        [Description("View information about a beatmap(set).")]
        public async Task<IDiscordCommandResult> Exec(
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

        private IDiscordCommandResult Beatmapset(APIBeatmapSet beatmapset)
            => Menu(new DefaultTextMenu(new BeatmapsetPagedView(beatmapset)));

        private IDiscordCommandResult BeatmapSingle(APIBeatmapSet beatmapset, int beatmapId)
        {
            SetBeatmapContext(beatmapId);

            var pageProvider = new BeatmapPageProvider(
                beatmapset,
                APIClientStore.GetClient(GameServer.Osu),
                modParserService,
                RulesetTypeParser.SupportedRulesets[beatmapset.Beatmaps.First(b => b.OnlineID == beatmapId).RulesetID]
            );
            return Menu(
                new DefaultTextMenu(
                    new BeatmapSingleView(pageProvider, beatmapId)
                )
            );
        }
    }
}