using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Pepper.Database
{
    public class RestrictedCommandWhitelistProvider : DbContext
    {
        [Table("command-whitelist")]
        private class Record
        {
            [Column("command_id")] public string CommandIdentifier { get; set; } = null!;
            [Column("guild_id")] public string GuildId { get; set; } = null!;
        }

        public RestrictedCommandWhitelistProvider(DbContextOptions<RestrictedCommandWhitelistProvider> options) : base(options) { }
        private DbSet<Record> Records { get; set; } = null!;

        private static readonly ILogger Log = Serilog.Log.Logger.ForContext<RestrictedCommandWhitelistProvider>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Record>(b =>
            {
                b.Property(b => b.CommandIdentifier).HasColumnName("command_id");
                b.Property(b => b.GuildId).HasColumnName("guild_id");
                b.HasKey(r => new { r.CommandIdentifier, r.GuildId });
            });
        }

        public async Task<bool> IsAllowedGuild(string guildId, string commandIdentifier)
        {
            var res = await Records.AnyAsync(r => r.CommandIdentifier == commandIdentifier && r.GuildId == guildId);
            Log.Information("Checking if {0} could be called in guild {1} : {2}", commandIdentifier, guildId, res);
            return res;
        }

        public async Task<bool> RemoveAllowedGuild(string guildId, string commandIdentifier)
        {
            var entity = await Records.FirstOrDefaultAsync(r => r.CommandIdentifier == commandIdentifier && r.GuildId == guildId);
            if (entity != null)
            {
                Records.Remove(entity);
                Log.Information("Disabling command {0} in guild {1}", commandIdentifier, guildId);
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

            Log.Information("Allowing command {0} in guild {1}", commandIdentifier, guildId);
            return await SaveChangesAsync() != 0;
        }
    }
}