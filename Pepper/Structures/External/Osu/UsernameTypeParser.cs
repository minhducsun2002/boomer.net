using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord.Bot.Commands;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commands.Osu;
using Pepper.Commons.Interfaces.FailureFormattable;
using Pepper.Commons.Structures;
using Pepper.Database.OsuUsernameProviders;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class UsernameTypeParser : DiscordTypeParser<Username>, IFailureFormattableTypeParser
    {
        public override async ValueTask<ITypeParserResult<Username>> ParseAsync(IDiscordCommandContext context, IParameter parameter, ReadOnlyMemory<char> input)
        {
            string uidToLookup;

            if (input.Length != 0)
            {
                var value = new string(input.Span);
                var match = Regex.Match(value, @"^<@!?(\d+)>$",
                    RegexOptions.ECMAScript | RegexOptions.Compiled | RegexOptions.CultureInvariant);
                if (!match.Success)
                {
                    return Success(new Username
                    {
                        OsuUsername = value,
                        RippleUsername = value,
                        DiscordUserId = context.Author.Id.ToString()
                    });
                }

                uidToLookup = match.Groups[1].Value;
            }
            else
            {
                uidToLookup = context.Author.Id.ToString();
            }


            var username = await context.Services.GetRequiredService<IOsuUsernameProvider>().GetUsernames(uidToLookup);
            if (username != null)
            {
                return Success(username);
            }

            if (parameter.GetTypeInformation().IsOptional)
            {
                return Success(null!);
            }

            var saveHintText = "";
            var saveCommand = context.Bot.Commands.EnumerateTextCommands()
                .FirstOrDefault(command => command.CustomAttributes.OfType<SaveUsernameAttribute>().Any());
            if (saveCommand != default && await saveCommand.RunChecksAsync(context) is SuccessfulResult)
            {
                saveHintText =
                    $" Use \"{saveCommand.GetPrimaryInvocation(context.Bot)}\" to set it up.";
            }

            return Failure(
                "An username wasn't specified and couldn't find a saved username for you."
                + saveHintText
            );
        }
    }
}