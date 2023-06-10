using Pepper.Commons.Maimai;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Pepper.Frontends.Maimai.Services;

namespace Pepper.Frontends.Maimai.Commands.Button
{
    public class Friend : MaimaiButtonCommand
    {
        public Friend(MaimaiDxNetClientFactory f, MaimaiDataService d, IMaimaiDxNetCookieProvider c) : base(f, d, c) { }
    }
}