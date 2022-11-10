using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pepper.Commons.Maimai.Entities
{
    [Table("artist")]
    public class Artist
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string? Name { get; set; }

        [InverseProperty("Artist")]
        public virtual ICollection<Song>? Songs { get; set; }
    }
}
