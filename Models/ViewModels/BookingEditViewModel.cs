using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models.ViewModels
{
    public class BookingEditViewModel
    {
        public int Id { get; set; }
        public string? BookingReference { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        public string? AdditionalNotes { get; set; }

        public bool IsPickupDelivery { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }
        
        public string? VehicleInfo { get; set; }
        public string? ServicesInfo { get; set; }
    }
}
