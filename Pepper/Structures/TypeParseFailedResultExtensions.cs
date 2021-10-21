using System.Linq;
using Disqord;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Structures
{
    public static class TypeParseFailedResultExtensions
    {
        public static bool TryFormatFailure(this TypeParseFailedResult typeParseFailedResult, out LocalMessage? formattedMessage)
        {
            formattedMessage = default;
            var parameter = typeParseFailedResult.Parameter;
            var formatter = parameter.Attributes.OfType<FormatTypeParseFailureAttribute>().FirstOrDefault();
            formattedMessage = formatter?.Format(typeParseFailedResult);
            return formattedMessage != null;
        }
    }
}