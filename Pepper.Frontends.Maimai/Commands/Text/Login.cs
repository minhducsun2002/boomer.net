using Disqord;
using Disqord.Bot.Commands;
using Disqord.Rest;
using Pepper.Commons.Maimai;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;
using Qmmands;
using Qmmands.Text;

namespace Pepper.Frontends.Maimai.Commands.Text
{
    public class Login : MaimaiTextCommand
    {
        public Login(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookieProvider)
            : base(factory, data, cookieProvider) { }

        [TextCommand("mailogin", "login")]
        [Description("Provide your cookie here.")]
        [RequirePrivate]
        public async Task<IDiscordCommandResult?> Exec([Description("Self-explanatory.")] string? cookie = null)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                return Reply(
                    new LocalMessage()
                        .WithContent("```js" +
                                     "\njavascript:prompt(" +
                                     "'Please copy this and enter into chat :','m!login '+document.cookie.split(';').map(a=>a.trim()).filter(a=>a.startsWith('clal='))[0].substr(5));" +
                                     "```")
                        .WithEmbeds(
                            new LocalEmbed()
                                .WithFields(
                                    new LocalEmbedField()
                                        .WithName("Step 1")
                                        .WithValue("Create a bookmarklet in your browser.\nUse the above text string as URL."),
                                    new LocalEmbedField()
                                        .WithName("Step 2")
                                        .WithValue("[Log into maimaiDX NET](https://maimaidx-eng.com/maimai-mobile/)."),
                                    new LocalEmbedField()
                                        .WithName("Step 3")
                                        .WithValue("Go to [this site](https://lng-tgk-aime-gw.am-all.net/common_auth/)."),
                                    new LocalEmbedField()
                                        .WithName("Step 4")
                                        .WithValue("Launch the bookmarklet you just created.\n" +
                                                   "Copy the resulting command and paste it here."),
                                    new LocalEmbedField()
                                        .WithName("For whoever thinks they know what they're doing")
                                        .WithValue("Step 1 and 4 can be skipped if you know what you're doing.\n" +
                                                   "Just paste the script into the address bar after step 3.\n" +
                                                   "Beware that Chrome strips the `javascript:` prefix - manually type it back."),
                                    new LocalEmbedField()
                                        .WithName("Important notice")
                                        .WithValue("Twitter integration seems to be broken these days. Log in **using other methods** (step 2) and try again.")
                                )
                                .WithFooter("It is advised that you perform step 2 onward using an incognito window.")
                        )
                );
            }
            await Context.Message.AddReactionAsync(Hourglass);
            long? friendCode;
            if ((friendCode = await TryCookie(cookie)) == null)
            {
                if (cookie.StartsWith("clal="))
                {
                    if ((friendCode = await TryCookie(cookie[5..])) == null)
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

            await CookieProvider.StoreCookie(Context.AuthorId, cookie, friendCode.Value);
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

        private async Task<long?> TryCookie(string cookie)
        {
            var client = ClientFactory.Create(cookie);
            try
            {
                return await client.GetUserFriendCode();
            }
            catch
            {
                return null;
            }
        }
    }
}