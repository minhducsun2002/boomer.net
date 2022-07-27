using System;
using System.Diagnostics;
using System.Linq;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Bot.Commands.Text;
using Pepper.Structures.Commands;
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
            if (result is CommandNotFoundResult)
            {
                return false;
            }

            if (result is ExceptionResult ex)
            {
                var stackTrace = new StackTrace(ex.Exception);
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
                    Title = ex.Exception.GetType().FullName!,
                    Description = $"`{ex.Exception.Message}`",
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

            if (result is ChecksFailedResult checksFailedResult)
            {
                var checks = checksFailedResult.FailedChecks;
                var types = checks.Keys.ToDictionary(c => c.GetType());

                if (types.ContainsKey(typeof(RequireBotOwnerAttribute)))
                {
                    messageBase.Content = "Sorry, only my owner can do that.";
                    return true;
                }

                if (types.ContainsKey(typeof(RequireGuildWhitelistAttribute)))
                {
                    messageBase.Content = "Guild-restricted command.";
                    return true;
                }

                var prefixCheckType = typeof(PrefixCheckAttribute);
                if (types.Count == 1 && types.First().Key == prefixCheckType)
                {
                    return false;
                }

                var firstValidCheck = checks.First(c => c.GetType() != prefixCheckType).Key;
                messageBase.Content = @$"A secret check named {
                    firstValidCheck.GetType().Name.Replace("Attribute", "", StringComparison.InvariantCultureIgnoreCase)} failed.";
                return true;
            }

            return base.FormatFailureMessage(context, messageBase, result);
        }
    }
}