using System;
using Disqord.Bot;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Services.Osu;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Commands.Osu
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SaveUsernameAttribute : Attribute { }

    public class SaveUsername : OsuCommand
    {
        public SaveUsername(APIService service) : base(service) { }

        [SaveUsername]
        [RequireGuildWhitelist("osu-user-set")]
        [Command("usersave", "save", "userset", "set")]
        [Description("Set the username to default to in your future requests.")]
        public DiscordCommandResult Exec([Remainder][Description("Username to save.")] string username)
        {
            var service = Context.Services.GetRequiredService<DiscordOsuUsernameLookupService>();
            var result = service.StoreUser(Context.Author.Id, username);

            return Reply($"{Context.Author.Mention} is now bound to username **`{result?.OsuUsername}`**.");
        }
    }
}