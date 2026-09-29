using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty; // Admin, Staff, User

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
