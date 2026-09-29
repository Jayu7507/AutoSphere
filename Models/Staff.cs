using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class Staff
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string Specialization { get; set; } = string.Empty; // e.g. Mechanic, Electrician

        [Required, MaxLength(15)]
        public string Phone { get; set; } = string.Empty;

        public int UserId { get; set; } // Links to User account (Role = Staff)
        public User? User { get; set; }

        public bool IsAvailable { get; set; } = true;
        
        // e.g. out of 5
        public decimal Rating { get; set; }

        public ICollection<StaffAssignment> Assignments { get; set; } = new List<StaffAssignment>();
    }
}
