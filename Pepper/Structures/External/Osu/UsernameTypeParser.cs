using System.Threading.Tasks;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services.Osu;
using Qmmands;

namespace Pepper.Structures.External.Osu
{
    public class Username
    {
        public string Content = "";
        public static implicit operator string(Username username) => username.Content;
        public static explicit operator Username(string username) => new Username { Content = username };
    }

    public class UsernameTypeParser : DiscordTypeParser<Username>
    {
        public override async ValueTask<TypeParserResult<Username>> ParseAsync(Parameter parameter, string value, DiscordCommandContext context)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return TypeParserResult<Username>.Successful((Username) value);
            var lookupService = context.Services.GetRequiredService<DiscordOsuUsernameLookupService>();
            var username = await lookupService.GetUser(context.Author.Id);
            if (username != null) return TypeParserResult<Username>.Successful((Username) username);
            return !parameter.IsOptional
                ? TypeParserResult<Username>.Failed($"{nameof(username)} must be specified and not be null")
                : TypeParserResult<Username>.Successful(null!);
        }
    }
}