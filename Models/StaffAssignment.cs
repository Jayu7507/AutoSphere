using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class StaffAssignment
    {
        [Key]
        public int Id { get; set; }

        public int BookingId { get; set; }
        public Booking? Booking { get; set; }

        public int StaffId { get; set; }
        public Staff? Staff { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public string? ServiceNotes { get; set; } // Note entered by the mechanic
        public string? PartsUsed { get; set; } // JSON or text

        [Column(TypeName = "decimal(10,2)")]
        public decimal AdditionalCost { get; set; }

        public DateTime? EstimatedCompletion { get; set; }
    }
}
