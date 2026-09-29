using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
