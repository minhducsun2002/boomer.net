using System;
using System.Threading.Tasks;
using Disqord.Bot;
using Pepper.Commons.Osu;
using Pepper.Database.OsuUsernameProviders;
using Pepper.Structures.Commands;
using Qmmands;

namespace Pepper.Commands.Osu
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SaveUsernameAttribute : Attribute { }

    public class SaveUsername : OsuCommand
    {
        private readonly IOsuUsernameProvider usernameProvider;
        public SaveUsername(APIClientStore service, IOsuUsernameProvider usernameProvider) : base(service)
        {
            this.usernameProvider = usernameProvider;
        }

        [SaveUsername]
        [RequireGuildWhitelist("osu-user-set")]
        [Command("usersave", "save", "userset", "set")]
        [Description("Set the username to default to in your future requests.")]
        public async Task<DiscordCommandResult> Exec([Remainder][Description("Username to save.")] string username)
        {
            return Reply("Work in progress");
            // var result = usernameProvider.StoreUsername(Context.Author.Id, username);
            //
            // return Reply($"{Context.Author.Mention} is now bound to username **`{result?.OsuUsername}`**.");
        }
    }
}