using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public interface IParameterCheckFailureFormatter
    {
        public LocalMessage? FormatFailure(ParameterChecksFailedResult parameterChecksFailedResult, DiscordCommandContext commandContext);
    }
}