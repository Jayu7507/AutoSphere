using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class NewsletterSubscriber
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;
    }
}
