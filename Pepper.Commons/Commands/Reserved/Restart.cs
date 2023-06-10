using Disqord.Bot.Commands;
using Pepper.Commons.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commons.Commands.Reserved
{
    public class Restart : ReservedCommand
    {
        private readonly HostService? g;
        public Restart(HostService? g = null)
        {
            this.g = g;
        }

        [TextCommand("restart")]
        [Description("Restart the bot")]
        public IDiscordCommandResult? Exec()
        {
            if (g == null)
            {
                return Reply("nah");
            }

#pragma warning disable CS4014
            Reply("right, let's see");
            Task.Run(() => g.HostWrapper?.Reload()).ConfigureAwait(false);
#pragma warning restore CS4014
            return null;
        }
    }
}