using System.Net.Http;
using Pepper.Commons.Maimai;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Pepper.Structures;
using Pepper.Structures.CommandAttributes.Metadata;

namespace Pepper.Commands.Maimai
{
    [Category("maimai")]
    public abstract class MaimaiCommand : Command
    {
        // TODO: Split game data DB requirement into another subclass. User needs no game data.
        protected readonly HttpClient HttpClient;
        protected readonly MaimaiDbContext GameDataContext;
        protected readonly IMaimaiDxNetCookieProvider CookieProvider;
        public MaimaiCommand(HttpClient httpClient, MaimaiDbContext maimaiDbContext, IMaimaiDxNetCookieProvider cookieProvider)
        {
            HttpClient = httpClient;
            GameDataContext = maimaiDbContext;
            CookieProvider = cookieProvider;
        }
    }
}