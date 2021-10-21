using Disqord.Bot;
using Pepper.Structures;
using Pepper.Structures.Commands;

namespace Pepper.Commands.Reserved
{
    [Category("Reserved")]
    [RequireBotOwner]
    public abstract class ReservedCommand : Command { }
}