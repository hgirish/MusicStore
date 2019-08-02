using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MusicStore.Models
{
    public class Album
    {
        [ScaffoldColumn(false)]
        public int AlbumId { get; set; }
        public int GenreId { get; set; }
        public int ArtistId { get; set; }
        [Required]
        [StringLength(160,MinimumLength =2)]
        public string Title { get; set; }
        [Required]
        [Range(0.01,100.00)]
        [DataType(DataType.Currency)]
        [Column(TypeName ="decimal(18,2)")]
        public decimal Price { get; set; }
        [Display(Name ="Album Art URL")]
        [StringLength(1024)]
        public string AlbumArtUrl { get; set; }
        public virtual Genre Genre { get; set; }
        public virtual Artist Artist { get; set; }
        public List<OrderDetail> OrderDetails { get; set; }
        [ScaffoldColumn(false)]
        [BindNever]
        [Required]
        public DateTime Created { get; set; }

        public Album()
        {
            OrderDetails = new List<OrderDetail>();
            Created = DateTime.UtcNow;
        }
    }
}
