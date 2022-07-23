using Disqord.Bot.Commands.Text;
using Pepper.Structures.Commands;

namespace Pepper.Structures
{
    [PrefixCheck]
    public abstract class BaseCommand<T> : DiscordTextModuleBase where T : IDiscordTextCommandContext { }

    public abstract class Command : BaseCommand<IDiscordTextCommandContext> { }
}