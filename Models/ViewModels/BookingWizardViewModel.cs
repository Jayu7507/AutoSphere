using System.Collections.Generic;
using System;
using System.ComponentModel.DataAnnotations;

namespace AutoSphere.Models.ViewModels
{
    public class BookingWizardViewModel
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        
        // --- Smart Selection Properties ---
        public List<Vehicle> UserVehicles { get; set; } = new List<Vehicle>();
        public int? SelectedVehicleId { get; set; } // 0 or null means "New Vehicle"

        // Manual fields (Optional if SelectedVehicleId is used)
        [Display(Name = "Vehicle Type")]
        public string? VehicleType { get; set; } // Car, Bike
        
        public string? Brand { get; set; }
        
        public string? ModelName { get; set; }
        
        public string? LicensePlate { get; set; }

        [Required]
        public List<int> SelectedServiceIds { get; set; } = new List<int>();
        public List<Service> AvailableServices { get; set; } = new List<Service>();

        public bool IsPickupDelivery { get; set; }
        public string? PickupAddress { get; set; }
        public string? DeliveryAddress { get; set; }

        public bool IsDoorstepService { get; set; }
        public string? ServiceAddress { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; }

        public string? AdditionalNotes { get; set; }
    }
}
