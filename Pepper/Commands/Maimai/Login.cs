using System.Net.Http;
using System.Threading.Tasks;
using Disqord.Bot.Commands;
using Pepper.Commons.Maimai;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Pepper.Services.Maimai;
using Qmmands.Text;

namespace Pepper.Commands.Maimai
{
    public class Login : MaimaiCommand
    {
        public Login(HttpClient http, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(http, data, cookieProvider) {}

        [TextCommand("mailogin")]
        [RequirePrivate]
        public async Task<IDiscordCommandResult> Exec(string? cookie = null)
        {
            if (cookie == null)
            {
                return Reply("Please provide a cookie.");
            }
            var client = new MaimaiDxNetClient(HttpClient, cookie);
            var uid = await client.VerifyCookie();
            if (uid == null)
            {
                return Reply("Please provide a valid cookie.");
            }

            await CookieProvider.StoreCookie(Context.AuthorId, cookie);
            return Reply("We're going! Your cookie was accepted.");
        }
    }
}