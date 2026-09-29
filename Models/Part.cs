using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class Part
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(50)]
        public string SKU { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public int ThresholdLevel { get; set; } = 5;

        public bool IsActive { get; set; } = true;
    }
}
