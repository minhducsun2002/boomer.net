using Disqord.Bot.Commands.Text;

namespace Pepper.Structures
{
    public abstract class BaseCommand<T> : DiscordTextModuleBase where T : IDiscordTextCommandContext { }

    public abstract class Command : BaseCommand<IDiscordTextCommandContext> { }
}