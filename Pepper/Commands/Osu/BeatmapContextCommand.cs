using Pepper.Commons.Osu;
using Pepper.Services.Osu;

namespace Pepper.Commands.Osu
{
    public abstract class BeatmapContextCommand : OsuCommand
    {
        protected BeatmapContextCommand(IAPIClient service, BeatmapContextProviderService b) : base(service) => beatmapContext = b;

        private readonly BeatmapContextProviderService beatmapContext;
        protected void SetBeatmapContext(int beatmapId) => beatmapContext.SetBeatmap(Context.ChannelId.ToString(), beatmapId);
    }
}