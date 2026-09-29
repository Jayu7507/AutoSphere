using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public string? IncludedFeatures { get; set; } // comma separated or JSON

        public string? ImageUrl1 { get; set; }
        public string? ImageUrl2 { get; set; }
        public string? ImageUrl3 { get; set; }

        public bool IsDoorstepAvailable { get; set; } = false;

        public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
    }
}
