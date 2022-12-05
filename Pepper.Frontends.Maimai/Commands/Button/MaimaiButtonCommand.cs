using Disqord.Bot.Commands.Components;
using Disqord.Bot.Commands.Text;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public abstract class MaimaiButtonCommand : DiscordComponentModuleBase
    {
        protected readonly HttpClient HttpClient;
        protected readonly IMaimaiDxNetCookieProvider CookieProvider;
        protected readonly MaimaiDataService GameDataService;

        protected MaimaiButtonCommand(HttpClient httpClient, MaimaiDataService data, IMaimaiDxNetCookieProvider cookie)
        {
            HttpClient = httpClient;
            CookieProvider = cookie;
            GameDataService = data;
        }
    }
}