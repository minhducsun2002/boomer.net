using Disqord.Bot.Commands;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.CommandAttributes.Metadata;
using Pepper.Structures;

namespace Pepper.Commands.Reserved
{
    [Category("Reserved")]
    [RequireBotOwner]
    [Hidden]
    public abstract class ReservedCommand : Command { }
}