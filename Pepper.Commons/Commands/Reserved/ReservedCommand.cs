using Disqord.Bot.Commands;
using Pepper.Commons.Structures;
using Pepper.Commons.Structures.CommandAttributes.Metadata;

namespace Pepper.Commons.Commands.Reserved
{
    [Category("Reserved")]
    [RequireBotOwner]
    [Hidden]
    public abstract class ReservedCommand : Command { }
}