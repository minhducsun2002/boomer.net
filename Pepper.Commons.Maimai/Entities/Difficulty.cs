using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pepper.Commons.Maimai.Entities
{
    [Table("difficulty")]
    public class Difficulty
    {
        [Key]
        [Column("path")] public string Path { get; set; } = null!;

        [Column("level")] public int Level { get; set; }

        [Column("level_decimal")] public int LevelDecimal { get; set; }

        [Column("max_notes")] public int MaxNotes { get; set; }

        [Column("enabled")] public bool Enabled { get; set; }

        [Column("song_id")] public int SongId { get; set; }

        [Column("order")] public int Order { get; set; }

        // [ForeignKey(nameof(SongId))]
        // public virtual Song? Song { get; set; }
    }
}