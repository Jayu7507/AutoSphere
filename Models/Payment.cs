using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        [Required, MaxLength(100)]
        public string RazorpayOrderId { get; set; } = string.Empty;

        public string? RazorpayPaymentId { get; set; }
        public string? RazorpaySignature { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }

        // Success, Failed, Pending, Refunded
        [Required, MaxLength(20)]
        public string Status { get; set; } = string.Empty;

        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty; // UPI, Card, NetBanking

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
