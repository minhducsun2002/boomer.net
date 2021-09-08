using Disqord;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public interface IParameterCheckWithFailureFormatter
    {
        public LocalMessage? FormatFailure(ParameterChecksFailedResult parameterChecksFailedResult);
    }
}