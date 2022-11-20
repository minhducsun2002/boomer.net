using Pepper.Commons.Osu;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Utils;

namespace Pepper.Frontends.Osu.Commands
{
    public abstract class BeatmapContextCommand : OsuCommand
    {
        protected BeatmapContextCommand(APIClientStore apiClientStore, BeatmapContextProviderService b) : base(apiClientStore) => beatmapContext = b;

        private readonly BeatmapContextProviderService beatmapContext;
        protected int? GetBeatmapIdFromContext()
        {
            var maybeMsg = Context.Message.ReferencedMessage;
            if (maybeMsg.HasValue && maybeMsg.Value != null)
            {
                var msg = maybeMsg.Value!;
                // there should only be a single embed
                var embed = msg.Embeds[0];
                if (embed.Url is not null)
                {
                    if (URLParser.CheckMapUrl(embed.Url, out _, out var id, out _) && id != null)
                    {
                        return id.Value;
                    }
                }
            }

            return beatmapContext.GetBeatmap(Context.ChannelId.ToString());
        }

        protected void SetBeatmapContext(int beatmapId) => beatmapContext.SetBeatmap(Context.ChannelId.ToString(), beatmapId);
    }
}