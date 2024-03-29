using Disqord.Bot.Commands;
using Pepper.Commons.Osu;
using Pepper.Frontends.Database.OsuUsernameProviders;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Osu.Commands
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
        [TextCommand("usersave", "save", "userset", "set")]
        [Description("Set the username to default to in your future requests.")]
        public IDiscordCommandResult Exec([Remainder][Description("Username to save.")] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return Reply("Please specify an username!");
            }

            return View(new ChooseServerView(
                Context.Author.Id.ToString(),
                username,
                Bot.Services
            ), TimeSpan.FromSeconds(30));
            // var result = usernameProvider.StoreUsername(Context.Author.Id, username);
            //
            // return Reply($"{Context.Author.Mention} is now bound to username **`{result?.OsuUsername}`**.");
        }
    }
}