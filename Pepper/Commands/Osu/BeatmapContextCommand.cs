using Pepper.Services.Osu;

namespace Pepper.Commands.Osu
{
    public abstract class BeatmapContextCommand : OsuCommand
    {
        protected BeatmapContextCommand(APIService service, BeatmapContextProviderService b) : base(service) => beatmapContext = b;

        private readonly BeatmapContextProviderService beatmapContext;
        protected void SetBeatmapContext(int beatmapId) => beatmapContext.SetBeatmap(Context.ChannelId.ToString(), beatmapId);
        protected int? GetBeatmapContext() => beatmapContext.GetBeatmap(Context.ChannelId.ToString());
    }
}