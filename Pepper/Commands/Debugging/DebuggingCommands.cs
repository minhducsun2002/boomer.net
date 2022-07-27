using Disqord.Bot.Commands;
using Pepper.Structures;
using Pepper.Structures.Commands;

namespace Pepper.Commands.Debugging
{
    [Hidden]
    [Category("Debugging")]
    [RequireBotOwner]
    public abstract class DebuggingCommand : Command { }
}