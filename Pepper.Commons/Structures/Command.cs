using Disqord;
using Disqord.Bot.Commands.Text;
using Disqord.Rest;

namespace Pepper.Commons.Structures
{
    public abstract class Command : DiscordTextModuleBase
    {
        protected async ValueTask<IUserMessage?> GetReferencedMessage()
        {
            var refMessage = Context.Message.Reference;
            if (refMessage == null)
            {
                return null;
            }

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

            return message;
        }
    }
}