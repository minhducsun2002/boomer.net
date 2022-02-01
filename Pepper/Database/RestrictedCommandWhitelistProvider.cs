using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Pepper.Database
{
    [Table("command-whitelist")]
    public class Record
    {
        [Column("command_id")] public string CommandIdentifier { get; set; }
        [Column("guild_id")] public string GuildId { get; set; }
    }

    public class RestrictedCommandWhitelistProvider : DbContext
    {
        public RestrictedCommandWhitelistProvider(DbContextOptions<RestrictedCommandWhitelistProvider> options) : base(options) { }
        public DbSet<Record> Records { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Record>(b =>
            {
                b.Property(b => b.CommandIdentifier).HasColumnName("command_id");
                b.Property(b => b.GuildId).HasColumnName("guild_id");
                b.HasKey(r => new { r.CommandIdentifier, r.GuildId });
            });
        }

        public Task<bool> IsAllowedGuild(string guildId, string commandIdentifier)
        {
            return Records.AnyAsync(r => r.CommandIdentifier == commandIdentifier && r.GuildId == guildId);
        }

        public async Task<bool> RemoveAllowedGuild(string guildId, string commandIdentifier)
        {
            var entity = await Records.FirstOrDefaultAsync(r => r.CommandIdentifier == commandIdentifier && r.GuildId == guildId);
            if (entity != null)
            {
                Records.Remove(entity);
                return await SaveChangesAsync() != 0;
            }

            return false;
        }

        public async Task<bool> AddAllowedGuild(string guildId, string commandIdentifier)
        {
            if (await IsAllowedGuild(guildId, commandIdentifier))
            {
                return true;
            }

            await Records.AddAsync(new Record
            {
                CommandIdentifier = commandIdentifier,
                GuildId = guildId
            });

            return await SaveChangesAsync() != 0;
        }
    }
}