using System.Linq;
using Disqord;
using Disqord.Bot;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Structures
{
    public static class ParameterChecksFailedResultExtensions
    {
        public static bool TryFormatFailure(
            this ParameterChecksFailedResult parameterChecksFailedResult,
            DiscordCommandContext commandContext,
            out LocalMessage? formattedMessage)
        {
            formattedMessage = default;
            var formatter = parameterChecksFailedResult.Parameter.Attributes
                .OfType<IParameterCheckFailureFormatter>()
                .FirstOrDefault();
            formattedMessage = formatter?.FormatFailure(parameterChecksFailedResult, commandContext);
            return formattedMessage != null;
        }
    }
}