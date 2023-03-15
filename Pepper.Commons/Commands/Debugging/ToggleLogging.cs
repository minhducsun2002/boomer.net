using Disqord;
using Disqord.Bot.Commands;
using Pepper.Commons.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commons.Commands.Debugging
{
    public class ToggleLogging : DebuggingCommand
    {
        private readonly LoggingService loggingService;
        private static readonly LocalEmoji Enabled = LocalEmoji.Unicode("🔊"), Disabled = LocalEmoji.Unicode("🔇");
        public ToggleLogging(LoggingService service) => loggingService = service;

        [TextCommand("log", "logtoggle", "logging")]
        [Description("Toggle verbose logging.")]
        public IDiscordCommandResult Exec()
        {
            loggingService.Enabled = !loggingService.Enabled;
            return Reaction(loggingService.Enabled ? Enabled : Disabled);
        }
    }
}