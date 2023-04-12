using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pepper.Frontends.MaimaiStatistics.Structures
{
    [Table("progress_log")]
    public class ProgressRecord
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("friend_id")] public long FriendId { get; set; }

        [Column("class")] public int Class { get; set; }

        [Column("name")] public string Name { get; set; } = null!;

        [Column("rating")] public int Rating { get; set; }

        [Column("dan")] public int Dan { get; set; }

        [Column("timestamp")] public DateTimeOffset Timestamp { get; set; }
    }
}