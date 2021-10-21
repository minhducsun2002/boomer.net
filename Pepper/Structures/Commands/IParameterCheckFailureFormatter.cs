using Disqord;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public interface IParameterCheckFailureFormatter
    {
        public LocalMessage? FormatFailure(ParameterChecksFailedResult parameterChecksFailedResult);
    }
}