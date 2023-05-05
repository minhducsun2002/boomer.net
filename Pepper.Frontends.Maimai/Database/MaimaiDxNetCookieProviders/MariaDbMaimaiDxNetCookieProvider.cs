using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BitFaster.Caching.Lru;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pepper.Frontends.Maimai.Database.MaimaiDxNetCookieProviders;
using Serilog;

namespace Pepper.Frontends.Maimai.Database
{
    public class MariaDbMaimaiDxNetCookieProvider : DbContext, IMaimaiDxNetCookieProvider
    {
        [Table("maimai-cookie")]
        private class Record
        {
            [Key]
            [Column("discord_id")] public ulong DiscordId { get; set; }
            [Column("cookie")] public string Cookie { get; set; } = "";
            [Column("friend_id")] public long FriendId { get; set; }
        }

        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<MariaDbMaimaiDxNetCookieProvider>();

        public MariaDbMaimaiDxNetCookieProvider(DbContextOptions<MariaDbMaimaiDxNetCookieProvider> options) : base(options) { }
        private FastConcurrentLru<ulong, Record> cache = new(200);
        private DbSet<Record> DbSet { get; set; } = null!;

        public void FlushCache(ulong? discordId = null)
        {
            if (discordId == null)
            {
                cache = new FastConcurrentLru<ulong, Record>(200);
            }
            else
            {
                cache.TryRemove(discordId.Value);
            }
        }

        public async ValueTask<long?> GetFriendId(ulong discordId)
        {
            var r = await GetEntry(discordId);
            return r?.FriendId;
        }

        public async ValueTask<string?> GetCookie(ulong discordId)
        {
            var r = await GetEntry(discordId);
            return r?.Cookie;
        }

        private async ValueTask<Record?> GetEntry(ulong discordId)
        {
            if (cache.TryGet(discordId, out var ret))
            {
                return ret;
            }

            Log.Information("Didn't found a record for user {0}. Querying.", discordId);
            var res = await DbSet.FirstOrDefaultAsync(r => r.DiscordId == discordId);
            if (res != null)
            {
                cache.AddOrUpdate(discordId, res);
                return res;
            }
            return null;
        }

        public async Task StoreCookie(ulong discordId, string cookie, long friendId)
        {
            var existing = await DbSet.FirstOrDefaultAsync(r => r.DiscordId == discordId);
            EntityEntry<Record> entry;
            if (existing != null)
            {
                existing.Cookie = cookie;
                existing.FriendId = friendId;
                entry = Update(existing);
            }
            else
            {
                entry = Add(new Record
                {
                    Cookie = cookie,
                    DiscordId = discordId,
                    FriendId = friendId
                });
            }

            await SaveChangesAsync();
            cache.AddOrUpdate(discordId, entry.Entity);
        }
    }
}