using Disqord;
using Disqord.Bot.Commands.Text;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Interfaces.FailureFormattable;
using Qmmands;
using Qmmands.Default;

namespace Pepper.Commons.Structures
{
    public partial class Bot
    {
        private bool FormatTypeParseFailedResult(
            IDiscordTextCommandContext context,
            LocalMessageBase messageBase,
            TypeParseFailedResult result)
        {
            var p = result.Parameter;
            var parserProvider = (DefaultTypeParserProvider) Services.GetRequiredService<ITypeParserProvider>();
            var typeParser = parserProvider.GetParser(p);
            if (typeParser is IFailureFormattableTypeParser formattable)
            {
                return formattable.Format(messageBase, context, result);
            }

            messageBase.Content = result.FailureReason;

            return true;
        }
    }
}