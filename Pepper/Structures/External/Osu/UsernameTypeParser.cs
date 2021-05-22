using System;
using System.Threading.Tasks;
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

    public class UsernameTypeParser : TypeParser<Username>
    {
        private readonly DiscordOsuUsernameLookupService lookupService;
        public UsernameTypeParser(DiscordOsuUsernameLookupService lookupService) { this.lookupService = lookupService; }
        
        public override async ValueTask<TypeParserResult<Username>> ParseAsync(Parameter parameter, string value, Qmmands.CommandContext context)
        {
            if (!string.IsNullOrWhiteSpace(value))
                return TypeParserResult<Username>.Successful((Username) value);
            var username = await lookupService.GetUser(((CommandContext) context).Author.Id);
            if (username != null) return TypeParserResult<Username>.Successful((Username) username);
            return !parameter.IsOptional
                ? TypeParserResult<Username>.Failed($"{nameof(username)} must be specified and not be null")
                : TypeParserResult<Username>.Successful(null!);
        }
    }
}