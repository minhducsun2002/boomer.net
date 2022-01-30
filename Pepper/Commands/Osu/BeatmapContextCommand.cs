using Pepper.Commons.Osu;
using Pepper.Services.Osu;

namespace Pepper.Commands.Osu
{
    public abstract class BeatmapContextCommand : OsuCommand
    {
        protected BeatmapContextCommand(APIClientStore apiClientStore, BeatmapContextProviderService b) : base(apiClientStore) => BeatmapContext = b;

        protected readonly BeatmapContextProviderService BeatmapContext;
        protected void SetBeatmapContext(int beatmapId) => BeatmapContext.SetBeatmap(Context.ChannelId.ToString(), beatmapId);
    }
}