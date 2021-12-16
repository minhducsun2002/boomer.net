using System.Linq;
using Disqord;
using Disqord.Bot;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Structures
{
    public static class CommandExecutionFailedResultExtensions
    {
        /// <summary>
        /// Try formatting execution failures with the lowest containing module providing a formatter method.
        /// </summary>
        /// <remarks>
        /// This method tries to find the lowest module in the module hierachy of the failed command that implements
        ///     <see cref="ICommandExecutionFailureFormatter"/> and execute its failure formatter method..
        /// Note that if the chosen formatter returns <c>null</c> it will not attempt to go further up the chain
        /// and assumes that the formatting was failed instead, as if there was no formatter defined.
        /// </remarks>
        /// <param name="commandExecutionFailedResult">Failed result to try formatting.</param>
        /// <param name="message">The output formatted message, or <c>null</c> if this error is to be ignored.</param>
        /// <param name="context">The command context.</param>
        /// <returns>Whether the formatting succeeded.</returns>
        public static bool TryFormatFailure(
            this CommandExecutionFailedResult commandExecutionFailedResult,
            DiscordCommandContext context,
            out LocalMessage? message
        )
        {
            var command = commandExecutionFailedResult.Command;
            var module = command.Module;

            while (module != null)
            {
                // TODO: Switch the behaviour to support multiple failure formatters 
                var formatter = module.Attributes.OfType<ICommandExecutionFailureFormatter>().FirstOrDefault();
                if (formatter != default)
                {
                    if (formatter.TryFormatFailure(context, commandExecutionFailedResult, out message))
                    {
                        return true;
                    }
                }

                module = module.Parent;
            }

            message = null;
            return false;
        }
    }
}