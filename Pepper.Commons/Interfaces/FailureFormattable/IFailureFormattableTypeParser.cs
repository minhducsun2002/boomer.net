using Disqord;
using Disqord.Bot.Commands;
using Qmmands;

namespace Pepper.Commons.Interfaces.FailureFormattable
{
    public interface IFailureFormattableTypeParser
    {
        public bool Format(LocalMessageBase localMessageBase, IDiscordCommandContext context, IResult result)
        {
            if (result.FailureReason is not null)
            {
                localMessageBase.Content = result.FailureReason;
                return true;
            }

            return false;
        }
    }
}