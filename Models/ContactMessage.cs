using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class ContactMessage
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsResolved { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
