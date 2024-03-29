﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pepper.Commons.Maimai.Structures.Data.Enums;

namespace Pepper.Commons.Maimai.Entities
{
    [Table("songs")]
    public class Song : ISong
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }
        [Column("dataName")]
        [StringLength(300)]
        public string? DataName { get; set; }
        [Column("sortName")]
        public string? SortName { get; set; }
        [Column("name")]
        public string Name { get; set; } = null!;
        [Column("rightsInfoId")]
        public int? RightsInfoId { get; set; }
        [Column("artistId")]
        public int? ArtistId { get; set; }
        [Column("genreId")]
        public int? GenreId { get; set; }
        [Column("bpm")]
        public int? Bpm { get; set; }
        [Column("addVersionId")]
        public int AddVersionId { get; set; }

        [ForeignKey("AddVersionId")]
        [InverseProperty("Songs")]
        public virtual AddVersion? AddVersion { get; set; }
        [ForeignKey("ArtistId")]
        [InverseProperty("Songs")]
        public virtual Artist? Artist { get; set; }
        [ForeignKey("GenreId")]
        [InverseProperty("Songs")]
        public virtual Genre? Genre { get; set; }

        // [InverseProperty(nameof(Difficulty.Song))]
        public IReadOnlyList<Difficulty> Difficulties { get; set; } = new List<Difficulty>();

        public ChartVersion Version => Id > 10000 ? ChartVersion.Deluxe : ChartVersion.Standard;
    }
}
