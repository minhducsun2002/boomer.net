using System.Linq;
using Disqord;
using Disqord.Bot.Commands.Text;
using Qmmands;

namespace Pepper.Structures
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