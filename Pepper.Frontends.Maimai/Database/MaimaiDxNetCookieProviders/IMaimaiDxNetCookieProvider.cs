namespace Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders
{
    public interface IMaimaiDxNetCookieProvider
    {
        public ValueTask<string?> GetCookie(ulong discordId);
        public Task StoreCookie(ulong discordId, string cookie);
    }
}