using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class Coupon
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal MaxDiscountAmount { get; set; }

        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}
