using Disqord.Bot.Commands;
using Pepper.Commons.Structures;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Metadata;

namespace Pepper.Commands.Debugging
{
    [Hidden]
    [Category("Debugging")]
    [RequireBotOwner]
    public abstract class DebuggingCommand : Command { }
}