using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Pepper.Services.Maimai;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Commands.Maimai
{
    public class Login : MaimaiCommand
    {
        public Login(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(http, data, cookieProvider) { }

        private static readonly LocalEmoji Hourglass = new("⏳");
        private static readonly LocalEmoji Failed = new("❌");
        private static readonly LocalEmoji Success = new("✅");

        [TextCommand("mailogin")]
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