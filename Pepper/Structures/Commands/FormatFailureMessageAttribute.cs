using Disqord;
using Qmmands;

namespace Pepper.Structures.Commands
{
    public abstract class FormatTypeParseFailureAttribute : System.Attribute
    {
        public abstract LocalMessage Format(TypeParseFailedResult typeParseFailedResult);
    }
}