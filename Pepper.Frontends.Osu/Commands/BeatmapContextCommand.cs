using Pepper.Commons.Extensions;
using Pepper.Commons.Osu;
using Pepper.Frontends.Osu.Services;

namespace Pepper.Frontends.Osu.Commands
{
    public abstract class BeatmapContextCommand : OsuCommand
    {
        protected BeatmapContextCommand(APIClientStore apiClientStore, BeatmapContextProviderService b) : base(apiClientStore) => beatmapContext = b;

        private readonly BeatmapContextProviderService beatmapContext;
        protected ValueTask<int?> GetBeatmapIdFromContext() => Context.GetBeatmapIdFromContext();

        protected void SetBeatmapContext(int beatmapId) => beatmapContext.SetBeatmap(Context.ChannelId.ToString(), beatmapId);
    }
}