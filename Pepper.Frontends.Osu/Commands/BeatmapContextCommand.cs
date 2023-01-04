using Disqord;
using Disqord.Rest;
using Pepper.Commons.Osu;
using Pepper.Frontends.Osu.Services;
using Pepper.Frontends.Osu.Utils;

namespace Pepper.Frontends.Osu.Commands
{
    public abstract class BeatmapContextCommand : OsuCommand
    {
        protected BeatmapContextCommand(APIClientStore apiClientStore, BeatmapContextProviderService b) : base(apiClientStore) => beatmapContext = b;

        private readonly BeatmapContextProviderService beatmapContext;
        protected async ValueTask<int?> GetBeatmapIdFromContext()
        {
            var refMessage = Context.Message.Reference;
            if (refMessage != null)
            {
                var maybeMsg = Context.Message.ReferencedMessage;
                IUserMessage? message;
                if (!maybeMsg.HasValue)
                {
                    try
                    {
                        var msg = await Context.Bot.FetchMessageAsync(refMessage.ChannelId, refMessage.MessageId!.Value);
                        message = msg as IUserMessage;
                    }
                    catch
                    {
                        message = null;
                    }
                }
                else
                {
                    message = maybeMsg.Value;
                }


                // there should only be a single embed
                var embed = message?.Embeds[0];
                if (embed?.Url != null)
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