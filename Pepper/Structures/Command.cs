using Pepper.Structures.Commands;
using Disqord.Bot;
using Qmmands;

namespace Pepper.Structures
{
    [IgnoresExtraArguments]
    [PrefixCheck]
    public abstract class Command : DiscordModuleBase {}
}