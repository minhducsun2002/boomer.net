using Disqord.Bot;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Structures
{
    [IgnoresExtraArguments]
    [PrefixCheck]
    public abstract class BaseCommand<T> : DiscordModuleBase<T> where T : DiscordCommandContext { }

    public abstract class Command : BaseCommand<DiscordCommandContext> { }
    public abstract class Command<T> : BaseCommand<T> where T : DiscordCommandContext { }
}