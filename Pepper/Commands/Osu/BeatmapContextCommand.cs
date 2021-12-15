using Pepper.Commons.Osu;
using Pepper.Services.Osu;

namespace Pepper.Commands.Osu
{
    public abstract class BeatmapContextCommand : OsuCommand
    {
        protected BeatmapContextCommand(APIClientStore apiClientStore, BeatmapContextProviderService b) : base(apiClientStore) => beatmapContext = b;

        private readonly BeatmapContextProviderService beatmapContext;
        protected void SetBeatmapContext(int beatmapId) => beatmapContext.SetBeatmap(Context.ChannelId.ToString(), beatmapId);
    }
}