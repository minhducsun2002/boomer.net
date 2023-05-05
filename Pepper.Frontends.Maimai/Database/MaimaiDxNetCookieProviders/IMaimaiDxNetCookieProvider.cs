namespace Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders
{
    public interface IMaimaiDxNetCookieProvider
    {
        public ValueTask<long?> GetFriendId(ulong discordId);
        public ValueTask<string?> GetCookie(ulong discordId);
        public Task StoreCookie(ulong discordId, string cookie, long friendId);
        public void FlushCache(ulong? discordId = null);
    }
}