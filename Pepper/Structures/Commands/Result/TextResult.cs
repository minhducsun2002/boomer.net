using Qmmands;

namespace Pepper.Structures.Commands.Result
{
    public class TextResult : CommandResult
    {
        public TextResult(string message) { MessageText = message; }
        
        public override bool IsSuccessful => true;
        public readonly string MessageText;
    }
}