using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using Microsoft.EntityFrameworkCore;
using Pepper.Database.MaimaiDxNetCookieProviders;
using Serilog;

namespace Pepper.Database
{
    public class MariaDbMaimaiDxNetCookieProvider : DbContext, IMaimaiDxNetCookieProvider
    {
        [Table("maimai-cookie")]
        private class Record
        {
            [Key]
            [Column("discord_id")] public ulong DiscordId { get; set; }
            [Column("cookie")] public string Cookie { get; set; }
        }
        
        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<MariaDbMaimaiDxNetCookieProvider>();
        
        public MariaDbMaimaiDxNetCookieProvider(DbContextOptions<MariaDbMaimaiDxNetCookieProvider> options) : base(options) { }
        private readonly FastConcurrentLru<ulong, string> cache = new(200);
        private DbSet<Record> DbSet { get; set; } = null!;
        
        public async ValueTask<string?> GetCookie(ulong discordId)
        {
            if (cache.TryGet(discordId, out var ret))
            {
                return ret;
            }
            
            Log.Information("Didn't found a record for user {0}. Querying.", discordId);
            var res = await DbSet.FirstOrDefaultAsync(r => r.DiscordId == discordId);
            if (res != null)
            {
                cache.AddOrUpdate(discordId, res.Cookie);
                return res.Cookie;
            }
            return null;
        }

        public async Task StoreCookie(ulong discordId, string cookie)
        {
            var existing = await DbSet.FirstOrDefaultAsync(r => r.DiscordId == discordId);
            if (existing != null)
            {
                existing.Cookie = cookie;
                Update(existing);
            }
            else
            {
                Add(new Record
                {
                    Cookie = cookie,
                    DiscordId = discordId
                });
            }

            await SaveChangesAsync();
            cache.AddOrUpdate(discordId, cookie);
        }
    }
}