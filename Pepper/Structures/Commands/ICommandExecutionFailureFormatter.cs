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
        /// <param name="commandExecutionFailedResult">The result to format.</param>
        /// <param name="context">The command context.</param>
        /// <returns>A message representing the failure, or <c>null</c> to use the global behaviour of the bot.</returns>
        public LocalMessage? FormatFailure(DiscordCommandContext context, CommandExecutionFailedResult commandExecutionFailedResult);
    }
}