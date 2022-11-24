using Disqord.Bot.Commands;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands
{
    public class Login : MaimaiCommand
    {
        public Login(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(http, data, cookieProvider) { }

        [TextCommand("mailogin", "login")]
        [Description("Provide your cookie here.")]
        [RequirePrivate]
        public async Task<IDiscordCommandResult?> Exec([Description("Self-explanatory.")] string? cookie = null)
        {
            if (cookie == null)
            {
                return Reply("Please provide a cookie.");
            }
            await Context.Message.AddReactionAsync(Hourglass);
            if (!await TryCookie(cookie))
            {
                if (cookie.StartsWith("clal="))
                {
                    if (!await TryCookie(cookie[5..]))
                    {
                        return await ReactFailed();
                    }
                    cookie = cookie[5..];
                }
                else
                {
                    return await ReactFailed();
                }
            }

            await CookieProvider.StoreCookie(Context.AuthorId, cookie);
            await Context.Message.AddReactionAsync(Success);
            await Context.Message.RemoveOwnReactionAsync(Hourglass);
            return null;
        }

        private async Task<IDiscordCommandResult?> ReactFailed()
        {
            await Context.Message.AddReactionAsync(Failed);
            await Context.Message.RemoveOwnReactionAsync(Hourglass);
            return null;
        }

        private async Task<bool> TryCookie(string cookie)
        {
            var client = new MaimaiDxNetClient(HttpClient, cookie);
            return await client.VerifyCookie() != null;
        }
    }
}