using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Qmmands;

namespace Pepper.Structures
{
    public partial class Bot
    {
        protected override bool FormatFailureMessage(IDiscordCommandContext ctx, LocalMessageBase message, IResult result)
        {
            if (ctx is IDiscordTextCommandContext context)
            {
                return FormatTextCommandFailureMessage(context, message, result);
            }

            return base.FormatFailureMessage(ctx, message, result);
        }

        private bool FormatTextCommandFailureMessage(IDiscordTextCommandContext context, LocalMessageBase messageBase, IResult result)
        {
            return result switch
            {
                CommandNotFoundResult => false,
                ExceptionResult exception => FormatExceptionResult(context, messageBase, exception),
                ChecksFailedResult checksFailed => FormatChecksFailedResult(context, messageBase, checksFailed),
                ParameterChecksFailedResult parameterChecksFailed => FormatParameterChecksFailedResult(context, messageBase, parameterChecksFailed),
                TypeParseFailedResult typeParseFailed => FormatTypeParseFailedResult(context, messageBase, typeParseFailed),
                _ => base.FormatFailureMessage(context, messageBase, result)
            };
        }
    }
}