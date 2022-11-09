using Microsoft.EntityFrameworkCore;
using Pepper.Commons.Maimai.Entities;

namespace Pepper.Commons.Maimai
{
    public class MaimaiDataDbContext : DbContext
    {
        public MaimaiDataDbContext(DbContextOptions<MaimaiDataDbContext> options) : base(options) { }

        public virtual DbSet<AddVersion> AddVersions { get; set; } = null!;
        public virtual DbSet<Artist> Artists { get; set; } = null!;
        public virtual DbSet<Genre> Genres { get; set; } = null!;
        public virtual DbSet<Song> Songs { get; set; } = null!;
        public virtual DbSet<Difficulty> Difficulties { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Song>(entity =>
            {
                entity.HasOne(d => d.AddVersion)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.AddVersionId);

                entity.HasOne(d => d.Artist)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.ArtistId);

                entity.HasOne(d => d.Genre)
                    .WithMany(p => p.Songs)
                    .HasForeignKey(d => d.GenreId);

                entity.HasMany(s => s.Difficulties)
                    .WithOne(p => p.Song);
            });

            modelBuilder.Entity<Difficulty>(entity =>
            {
                entity.HasOne(d => d.Song)
                    .WithMany(s => s.Difficulties)
                    .HasForeignKey(d => d.SongId);
            });
        }
    }
}