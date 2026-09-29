using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class OTPVerification
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string OTP { get; set; } = string.Empty;

        public DateTime ExpiryTime { get; set; }
        public bool IsUsed { get; set; }
        public int Attempts { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
