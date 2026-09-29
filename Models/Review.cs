using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string? Comment { get; set; }

        public bool IsApproved { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
