using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commands.Osu;
using Pepper.Database.OsuUsernameProviders;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class UsernameTypeParser : DiscordTypeParser<Username>
    {
        public override async ValueTask<TypeParserResult<Username>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            string uidToLookup;

            if (!string.IsNullOrWhiteSpace(value))
            {
                var match = Regex.Match(value, @"^<@!?(\d+)>$",
                    RegexOptions.ECMAScript | RegexOptions.Compiled | RegexOptions.CultureInvariant);
                if (!match.Success)
                {
                    return TypeParserResult<Username>.Successful(new Username
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
                return TypeParserResult<Username>.Successful(username);
            }

            if (parameter.IsOptional)
            {
                return Success(null!);
            }

            var saveHintText = "";
            var saveCommand = context.Command.Service.GetAllCommands()
                .FirstOrDefault(command => command.Attributes.OfType<SaveUsernameAttribute>().Any());
            if (saveCommand != default && await saveCommand.RunChecksAsync(context) is SuccessfulResult)
            {
                saveHintText =
                    $"\nUse \"{saveCommand.GetPrefixes(context.Bot).First()}{saveCommand.Aliases[0]}\" to set it up.";
            }

            return TypeParserResult<Username>.Failed(
                "An username wasn't specified and couldn't find a saved username for you."
                + saveHintText
            );
        }
    }
}