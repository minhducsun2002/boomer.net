using Pepper.Structures.Commands;
using Pepper.Structures.Commands.Result;
using Qmmands;
using Command = Pepper.Structures.Command;

namespace Pepper.Commmands.General
{
    public class Ping : GeneralCommand
    {
        [Command("ping")]
        [Description("Pong!")]
        public TextResult Exec()
        {
            return new TextResult($"Pong!\nHeartbeat roundtrip latency : {Context.Client.Latency}ms.");
        }
    }
}