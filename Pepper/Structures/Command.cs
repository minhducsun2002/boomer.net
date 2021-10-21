using Disqord.Bot;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Structures
{
    [IgnoresExtraArguments]
    [PrefixCheck]
    public abstract class Command : DiscordModuleBase { }
}