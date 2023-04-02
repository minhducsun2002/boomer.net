using Disqord.Bot.Commands.Components;
using Disqord.Bot.Commands.Text;
using Pepper.Commons.Maimai;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public abstract class MaimaiButtonCommand : DiscordComponentModuleBase
    {
        protected readonly MaimaiDxNetClientFactory ClientFactory;
        protected readonly IMaimaiDxNetCookieProvider CookieProvider;
        protected readonly MaimaiDataService GameDataService;

        protected MaimaiButtonCommand(MaimaiDxNetClientFactory factory, MaimaiDataService data, IMaimaiDxNetCookieProvider cookie)
        {
            ClientFactory = factory;
            CookieProvider = cookie;
            GameDataService = data;
        }
    }
}