using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commands.Osu;
using Pepper.Services.Osu;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class Username
    {
        public string Content = "";
        public static implicit operator string(Username username) => username.Content;
        public static explicit operator Username(string username) => new() { Content = username };
    }

    public class UsernameTypeParser : DiscordTypeParser<Username>
    {
        public static UsernameTypeParser Instance = new();
        
        public override async ValueTask<TypeParserResult<Username>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            ulong uidToLookup;

            if (!string.IsNullOrWhiteSpace(value))
            {
                var match = Regex.Match(value, @"^<@!?(\d+)>$",
                    RegexOptions.ECMAScript | RegexOptions.Compiled | RegexOptions.CultureInvariant);
                if (!match.Success) return TypeParserResult<Username>.Successful((Username) value);
                uidToLookup = ulong.Parse(match.Groups[1].Value);
            }
            else uidToLookup = context.Author.Id;

            var lookupService = context.Services.GetRequiredService<DiscordOsuUsernameLookupService>();
            var username = await lookupService.GetUser(uidToLookup);
            if (username != null) return TypeParserResult<Username>.Successful((Username) username);
            if (parameter.IsOptional)
            {
                return Success(null!);
            }
            
            var saveHintText = "";
            var saveCommand = context.Command.Service.GetAllCommands()
                .FirstOrDefault(command => command.Attributes.OfType<SaveUsernameAttribute>().Any());
            if (saveCommand != default && await saveCommand.RunChecksAsync(context) == CheckResult.Successful)
            {
                saveHintText =
                    $"\nUse \"{saveCommand.GetPrefixes(context.Bot).First()}{saveCommand.Aliases[0]}\" with your username to set it up.";
            }
            
            return TypeParserResult<Username>.Failed(
                "An username wasn't specified and couldn't find a saved username for you."
                + saveHintText
            );
        }
    }
}