using Disqord;
using Disqord.Bot.Commands.Text;
using Disqord.Rest;

namespace Pepper.Commons.Extensions
{
    public static class DiscordTextCommandContextExtensions
    {
        public static async ValueTask<IUserMessage?> GetReferencedMessage(this IDiscordTextCommandContext context)
        {
            var refMessage = context.Message.Reference;
            if (refMessage == null)
            {
                return null;
            }

            var maybeMsg = context.Message.ReferencedMessage;
            IUserMessage? message;
            if (!maybeMsg.HasValue)
            {
                try
                {
                    var msg = await context.Bot.FetchMessageAsync(refMessage.ChannelId, refMessage.MessageId!.Value);
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

            return message;
        }
    }
}