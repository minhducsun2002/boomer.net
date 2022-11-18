using Disqord.Bot.Commands;
using Pepper.Commons.Structures;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Metadata;

namespace Pepper.Commands.Reserved
{
    [Category("Reserved")]
    [RequireBotOwner]
    [Hidden]
    public abstract class ReservedCommand : Command { }
}