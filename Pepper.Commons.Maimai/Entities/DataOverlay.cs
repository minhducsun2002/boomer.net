using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.Entities
{
    /// <summary>
    /// Complementary data for songs without entries in the main database
    /// </summary>
    [Table("data_overlay")]
    public class DataOverlay : ISong
    {
        [Key]
        [Column("id")]
        public int Identifier { get; set; }

        public int Id => 0;

        [Column("name")]
        public string Name { get; set; }

        [Column("chart_version")]
        public ChartVersion Version { get; set; }

        [Column("level")]
        public int Level { get; set; }

        [Column("level_decimal")]
        public int LevelDecimal { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("genreId")]
        public int GenreId { get; set; }

        [Column("artistId")]
        public int ArtistId { get; set; }

        [Column("addVersionId")]
        public int AddVersionId { get; set; }

        [Column("enabled")]
        public bool Enabled { get; set; }

        public Structures.Data.Enums.Difficulty Difficulty => (Structures.Data.Enums.Difficulty) Order;
    }
}