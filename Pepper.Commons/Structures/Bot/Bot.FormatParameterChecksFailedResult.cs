using Disqord;
using Disqord.Bot.Commands.Text;
using Pepper.Commons.Interfaces.FailureFormattable;
using Qmmands;

namespace Pepper.Commons.Structures
{
    public partial class Bot
    {
        private bool FormatParameterChecksFailedResult(
            IDiscordTextCommandContext context,
            LocalMessageBase messageBase,
            ParameterChecksFailedResult result)
        {
            foreach (var (c, _) in result.FailedChecks)
            {
                if (c is IFailureFormattableParameterCheck formattable)
                {
                    return formattable.Format(messageBase, context, result);
                }
            }
            return false;
        }
    }
}