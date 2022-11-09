using System.Diagnostics;
using System.Linq;
using Disqord;
using Disqord.Bot.Commands.Text;
using Pepper.Commons;
using Qmmands;

namespace Pepper.Structures
{
    public partial class Bot
    {
        private bool FormatExceptionResult(IDiscordTextCommandContext context, LocalMessageBase messageBase, ExceptionResult result)
        {
            if (result.Exception is IFriendlyException friendlyException)
            {
                messageBase.Content = friendlyException.FriendlyMessage;
                return true;
            }
            
            var stackTrace = new StackTrace(result.Exception);
            var frames = stackTrace.GetFrames()
                .Take(4)
                .Select(f =>
                {
                    var method = f.GetMethod()!;
                    var @class = method.ReflectedType!.FullName;
                    var name = method.Name;
                    var callsite = $"`{@class}.{name}()`";
                    if (method.ReflectedType.Namespace?.StartsWith(nameof(Pepper)) == true)
                    {
                        callsite = "__" + callsite + "__";
                    }
                    return callsite;
                })
                .ToArray();

            messageBase.Content = FormatFailureReason(context, result);
            messageBase.AddEmbed(new LocalEmbed
            {
                Title = result.Exception.GetType().FullName!,
                Description = $"`{result.Exception.Message}`",
                Fields = new LocalEmbedField[]
                {
                    new()
                    {
                        Name = "Thrown from",
                        Value = string.Join('\n', frames)
                    }
                },
                Footer = new LocalEmbedFooter { Text = $"Command : {context.Command!.Name} | Prefix : {context.Prefix}" },
            });
            return true;
        }
    }
}