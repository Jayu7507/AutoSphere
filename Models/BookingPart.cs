using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class BookingPart
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int PartId { get; set; }
        public Part? Part { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPriceAtBooking { get; set; }
        
        public System.DateTime AddedAt { get; set; } = System.DateTime.UtcNow;
    }
}
