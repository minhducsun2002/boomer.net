using System;
using Disqord.Bot.Commands;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commands.Debugging
{
    public class Throw : DebuggingCommand
    {
        [TextCommand("throw")]
        [Description("This command will always throw.")]
        public IDiscordCommandResult Exec()
        {
            throw new Exception("Manually thrown exception from command");
        }
    }
}