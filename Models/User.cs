using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
    
        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
    
        [Required, EmailAddress, MaxLength(150)]
        public string Email { get; set; } = string.Empty;
    
        [Required, MaxLength(15)]
        public string Phone { get; set; } = string.Empty;
    
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
    
        public int RoleId { get; set; }
        public Role? Role { get; set; }
        
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
