using System.Collections.Generic;
using System.Threading.Tasks;
using BitFaster.Caching.Lru;
using Microsoft.EntityFrameworkCore;

namespace Pepper.Database.OsuUsernameProviders
{
    public class MariaDbOsuUsernameProvider : DbContext, IOsuUsernameProvider
    {
        private readonly FastConcurrentLru<string, Username> cache = new(200);
        public MariaDbOsuUsernameProvider(DbContextOptions<MariaDbOsuUsernameProvider> opt) : base(opt) { }

        public DbSet<Username> DbSet { get; set; } = null!;

        private void Cache(string discordId, Username record)
        {
            Entry(record).State = EntityState.Detached;
            cache.AddOrUpdate(discordId, record);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Username>(b =>
            {
                b.Property(u => u.DefaultServerString).HasColumnName("default_mode");
                b.Property(u => u.DiscordUserId).HasColumnName("discord_user_id");
                b.Property(u => u.OsuUsername).HasColumnName("osu_username");
                b.Property(u => u.RippleUsername).HasColumnName("ripple_username");
            });
        }

        public async ValueTask<Username?> GetUsernames(string discordId)
        {
            if (cache.TryGet(discordId, out var @return))
            {
                return @return;
            }

            var record = await DbSet.FirstOrDefaultAsync(rec => rec.DiscordUserId == discordId);
            if (record != default)
            {
                Cache(discordId, record);
            }
            return record;
        }

        public async ValueTask<Dictionary<string, Username>> GetUsernamesBulk(params string[] discordUserId)
        {
            var result = new Dictionary<string, Username>();
            foreach (var uid in discordUserId)
            {
                if (cache.TryGet(uid, out var found))
                {
                    result[uid] = found;
                }
                else
                {
                    var record = await DbSet.FirstOrDefaultAsync(rec => rec.DiscordUserId == uid);
                    if (record != default)
                    {
                        Cache(uid, record);
                        result[uid] = record;
                    }
                }
            }

            return result;
        }

        public async Task<Username> StoreUsername(Username record)
        {
            cache.TryRemove(record.DiscordUserId);
            var existingRecord = await DbSet.FirstOrDefaultAsync(rec => rec.DiscordUserId == record.DiscordUserId);
            if (existingRecord != default)
            {
                existingRecord.OsuUsername = record.OsuUsername ?? existingRecord.OsuUsername;
                existingRecord.RippleUsername = record.RippleUsername ?? existingRecord.RippleUsername;
                Update(existingRecord);
            }
            else
            {
                Add(record);
            }
            await SaveChangesAsync();
            Cache(record.DiscordUserId, record);
            return record;
        }
    }
}