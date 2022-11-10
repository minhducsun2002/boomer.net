using System.Net.Http;
using System.Threading.Tasks;
using Disqord;
using Disqord.Bot.Commands;
using Disqord.Gateway;
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
            : base(http, data, cookieProvider) {}

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
            var client = new MaimaiDxNetClient(HttpClient, cookie);
            await Context.Message.AddReactionAsync(Hourglass);
            var uid = await client.VerifyCookie();
            if (uid == null)
            {
                await Context.Message.AddReactionAsync(Failed);
                await Context.Message.RemoveOwnReactionAsync(Hourglass);
                return null;
            }

            await CookieProvider.StoreCookie(Context.AuthorId, cookie);
            await Context.Message.AddReactionAsync(Success);
            await Context.Message.RemoveOwnReactionAsync(Hourglass);
            return null;
        }
    }
}