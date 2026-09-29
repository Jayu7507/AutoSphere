using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AutoSphere.Models;

namespace AutoSphere.Models.ViewModels
{
    public class WalkInBookingViewModel
    {
        // User Info
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        [EmailAddress]
        public string? Email { get; set; }

        // Vehicle Info
        public int? SelectedVehicleId { get; set; }
        
        [Required(ErrorMessage = "Vehicle Type is required")]
        public string VehicleType { get; set; } = "Car";

        [Required(ErrorMessage = "Brand is required")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model Name is required")]
        public string ModelName { get; set; } = string.Empty;

        [Required(ErrorMessage = "License Plate is required")]
        public string LicensePlate { get; set; } = string.Empty;

        // Booking Info
        public List<int> SelectedServiceIds { get; set; } = new List<int>();
        
        [DataType(DataType.Date)]
        public DateTime ScheduledDate { get; set; } = DateTime.Today;

        public string? AdditionalNotes { get; set; }

        public bool AutoAssignToMe { get; set; } = true;

        // Display Data (Repopulated on validation error)
        public List<Vehicle>? ExistingVehicles { get; set; }
        public List<Service>? AvailableServices { get; set; }
    }
}
