using System;
using System.Collections.Generic;
using System.Linq;
using Disqord;
using Disqord.Bot;
using Pepper.Structures;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper
{
    public partial class Pepper
    {
        protected override LocalMessage FormatFailureMessage(DiscordCommandContext context, FailedResult result)
        {
            if (result is CommandNotFoundResult) return null!;
        
            var content = "I'm sorry, an error occurred.";
            var embed = new LocalEmbed
            {
                Fields = new List<LocalEmbedField>(),
                Footer = new LocalEmbedFooter { Text = $"Command : {context.Command.Name} | Prefix : {context.Prefix}" },
                Timestamp = DateTimeOffset.Now
            };

            switch (result)
            {
                case CommandExecutionFailedResult executionFailedResult:
                    var exception = executionFailedResult.Exception;
                    var stackTrace = exception.StackTrace!.Split('\n');
                    
                    content = "I'm sorry, an error occurred executing your command.";
                    embed.Description = $"```{exception.Message}\n{string.Join('\n', stackTrace.Take(4))}```";
                    break;
                case TypeParseFailedResult typeParseFailedResult:
                {
                    if (typeParseFailedResult.TryFormatFailure(out var res))
                    {
                        return res!;
                    }
                    var parameter = typeParseFailedResult.Parameter;

                    content = "I'm sorry, an error occurred parsing your argument.";
                    embed.Fields = new List<LocalEmbedField>
                    {
                        new() { Name = "Parameter", Value = $"Name : `{parameter.Name}`\nType : `{parameter.Type.Name}`" },
                        new()
                        {
                            Name = "Parsing value",
                            Value = !string.IsNullOrEmpty(typeParseFailedResult.Value)
                                ? $"`{typeParseFailedResult.Value}`"
                                : "(empty value)"
                        }
                    };
                    if (!string.IsNullOrWhiteSpace(typeParseFailedResult.FailureReason))
                        embed.Fields.Add(new LocalEmbedField
                        {
                            Name = "Failure reason",
                            Value = typeParseFailedResult.FailureReason
                        });
                    break;
                }
                case ParameterChecksFailedResult parameterChecksFailedResult:
                {
                    if (parameterChecksFailedResult.TryFormatFailure(out var formatted))
                    {
                        return formatted!;
                    }

                    goto default;
                }
                case ChecksFailedResult checksFailedResult:
                {
                    var firstCheck = checksFailedResult.FailedChecks[0].Check;
                    switch (firstCheck)
                    {
                        case RequireBotOwnerAttribute:
                            content = "";
                            embed.Description = "This command can only be called by the bot owner.";
                            break;
                        case PrefixCheckAttribute:
                            return null!;
                        case RequireGuildWhitelistAttribute:
                            content = "";
                            embed.Description = "This command is restricted (whitelisted on a per-guild basis), hence not callable from this guild.";
                            break;
                        default:
                            embed.Description = $"Check {firstCheck.GetType().Name} failed.";
                            break;
                    }

                    break;
                }
                default:
                    embed.Description = result.FailureReason;
                    break;
            };

            return new LocalMessage
            {
                Content = string.IsNullOrWhiteSpace(content) ? null : content,
                Embeds = new List<LocalEmbed> { embed },
            }.WithReply(context.Message.Id);
        }
    }
}