using Disqord;
using Disqord.Bot;
using Qmmands;

namespace Pepper.Structures.Commands
{
    /// <summary>
    /// Interface for modules able to handle command execution failures in their (submodule's) commands.
    /// </summary>
    public interface ICommandExecutionFailureFormatter
    {
        /// <summary>
        /// Format a command execution failure result.
        /// </summary>
        /// <param name="context">The command context.</param>
        /// <param name="commandExecutionFailedResult">The result to format.</param>
        /// <param name="outputMessage">Output message if formatting succeeds, <c>null</c> otherwise.</param>
        /// <returns>Whether formatting succeeded.</returns>
        public bool TryFormatFailure(
            DiscordCommandContext context,
            CommandExecutionFailedResult commandExecutionFailedResult,
            out LocalMessage? outputMessage
        );
    }
}