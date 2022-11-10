using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pepper.Commons.Maimai.Entities
{
    [Table("add_version")]
    public class AddVersion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("name")]
        public string? Name { get; set; }

        [InverseProperty("AddVersion")]
        public virtual ICollection<Song>? Songs { get; set; }
    }
}