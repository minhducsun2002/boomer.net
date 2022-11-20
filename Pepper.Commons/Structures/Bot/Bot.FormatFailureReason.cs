using Disqord.Bot.Commands;
using Qmmands;

namespace Pepper.Commons.Structures
{
    public partial class Bot
    {
        protected override string? FormatFailureReason(IDiscordCommandContext context, IResult result)
        {
            if (result is ExceptionResult)
            {
                return "The command broke midway. It might have left a message.";
            }
            return base.FormatFailureReason(context, result);
        }
    }
}