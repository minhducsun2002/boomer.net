using System.Threading.Tasks;

namespace Pepper.Database.MaimaiDxNetCookieProviders
{
    public interface IMaimaiDxNetCookieProvider
    {
        public ValueTask<string?> GetCookie(ulong discordId);
        public Task StoreCookie(ulong discordId, string cookie);
    }
}