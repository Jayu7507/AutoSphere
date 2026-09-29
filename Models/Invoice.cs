using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class Invoice
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        [Required, MaxLength(30)]
        public string InvoiceNumber { get; set; } = string.Empty;

        public string FilePath { get; set; } = string.Empty; // Path to generated PDF

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
