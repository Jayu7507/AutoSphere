using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoSphere.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string BookingReference { get; set; } = string.Empty; // Unique ref (e.g. BKG12345)

        public int UserId { get; set; }
        public User? User { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public DateTime ScheduledDate { get; set; }
        
        // Allowed: Pending, Confirmed, InProgress, WaitingForParts, Completed, Cancelled
        [Required, MaxLength(30)]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal FinalAmount { get; set; } // Total - Discount

        public int? CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal AdditionalCharges { get; set; }

        public string? AdditionalNotes { get; set; }
        
        // --- Concierge Service ---
        public bool IsPickupDelivery { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }

        // --- Doorstep (Field) Service ---
        public bool IsDoorstepService { get; set; }
        public string? ServiceAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<BookingDetail> BookingDetails { get; set; } = new List<BookingDetail>();
        public ICollection<BookingPart> BookingParts { get; set; } = new List<BookingPart>();
        
        // 1 to 1 payment
        public Payment? Payment { get; set; }

        // M-to-1 if multiple mechanics handle a booking, or 1-to-1. We'll use StaffAssignment for flexibility
        public ICollection<StaffAssignment> StaffAssignments { get; set; } = new List<StaffAssignment>();
    }
}
