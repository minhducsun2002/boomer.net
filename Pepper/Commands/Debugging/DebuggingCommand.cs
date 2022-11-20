using Disqord.Bot.Commands;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.CommandAttributes.Metadata;

namespace Pepper.Commands.Debugging
{
    [Hidden]
    [Category("Debugging")]
    [RequireBotOwner]
    public abstract class DebuggingCommand : Command { }
}