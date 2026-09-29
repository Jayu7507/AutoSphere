using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class BookingDetail
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int ServiceId { get; set; }
        public Service? Service { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal PriceAtBooking { get; set; } // Snapshot of price
    }
}
