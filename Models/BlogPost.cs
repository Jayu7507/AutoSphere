using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Slug { get; set; } = string.Empty; // URL-friendly title

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Excerpt { get; set; } = string.Empty; // Short summary

        public string? ImageUrl { get; set; }

        public string Category { get; set; } = "Maintenance";

        public string AuthorName { get; set; } = "AutoSphere Team";

        public int ReadTimeMinutes { get; set; } = 5;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPublished { get; set; } = true;
    }
}
