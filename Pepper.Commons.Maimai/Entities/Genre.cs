using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pepper.Commons.Maimai.Entities
{
    [Table("genre")]
    public class Genre
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string Name { get; set; } = null!;

        [InverseProperty("Genre")]
        public virtual ICollection<Song>? Songs { get; set; }
    }
}
