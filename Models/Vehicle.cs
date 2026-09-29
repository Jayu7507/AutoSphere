using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Type { get; set; } = string.Empty; // Car, Bike

        [Required, MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ModelName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string LicensePlate { get; set; } = string.Empty;

        public int UserId { get; set; }
        public User? User { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
