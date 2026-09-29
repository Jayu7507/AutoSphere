using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using AutoSphere.Models.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AutoSphere.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly AppDbContext _context;

        public BookingController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Book(int serviceId)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return NotFound();

            var userVehicles = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            var model = new BookingWizardViewModel
            {
                ServiceId = service.Id,
                ServiceName = service.Name,
                Price = service.Price,
                ScheduledDate = DateTime.Today.AddDays(1),
                UserVehicles = userVehicles,
                AvailableServices = await _context.Services.ToListAsync(),
                SelectedServiceIds = new List<int> { service.Id }
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Book(BookingWizardViewModel model)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            if (ModelState.IsValid)
            {
                int vehicleId = 0;

                // Scenario A: Use Existing Vehicle
                if (model.SelectedVehicleId.HasValue && model.SelectedVehicleId.Value > 0)
                {
                    var existingVehicle = await _context.Vehicles
                        .FirstOrDefaultAsync(v => v.Id == model.SelectedVehicleId.Value && v.UserId == userId);
                    
                    if (existingVehicle == null)
                    {
                        ModelState.AddModelError("SelectedVehicleId", "Invalid vehicle selected.");
                    }
                    else
                    {
                        vehicleId = existingVehicle.Id;
                    }
                }
                // Scenario B: Register New Vehicle
                else
                {
                    if (string.IsNullOrEmpty(model.Brand) || string.IsNullOrEmpty(model.ModelName) || string.IsNullOrEmpty(model.LicensePlate))
                    {
                        ModelState.AddModelError("", "Please provide all vehicle details for the new registration.");
                    }
                    else
                    {
                        var vehicle = new Vehicle
                        {
                            Type = model.VehicleType ?? "Car",
                            Brand = model.Brand,
                            ModelName = model.ModelName,
                            LicensePlate = model.LicensePlate.ToUpper(),
                            UserId = userId
                        };
                        _context.Vehicles.Add(vehicle);
                        await _context.SaveChangesAsync();
                        vehicleId = vehicle.Id;
                    }
                }

                if (vehicleId > 0)
                {
                    // Calculate Total for all selected services
                    var services = await _context.Services
                        .Where(s => model.SelectedServiceIds.Contains(s.Id))
                        .ToListAsync();

                    if (!services.Any())
                    {
                        ModelState.AddModelError("SelectedServiceIds", "Please select at least one service.");
                    }
                    else
                    {
                        decimal totalBase = services.Sum(s => s.Price);

                        // Create Booking with uniqueness check
                        string reference;
                        do
                        {
                            reference = "BKG" + new Random().Next(10000, 99999).ToString();
                        } while (await _context.Bookings.AnyAsync(b => b.BookingReference == reference));

                        var booking = new Booking
                        {
                            BookingReference = reference,
                            UserId = userId,
                            VehicleId = vehicleId,
                            ScheduledDate = model.ScheduledDate,
                            Status = "Pending",
                            TotalAmount = totalBase,
                            DiscountAmount = 0,
                            FinalAmount = totalBase,
                            AdditionalNotes = model.AdditionalNotes,
                            IsPickupDelivery = model.IsPickupDelivery,
                            PickupAddress = model.PickupAddress,
                            DeliveryAddress = model.DeliveryAddress,
                            IsDoorstepService = model.IsDoorstepService,
                            ServiceAddress = model.ServiceAddress
                        };
                        
                        _context.Bookings.Add(booking);
                        await _context.SaveChangesAsync();

                        // Add Booking Details for EACH service
                        foreach (var s in services)
                        {
                            var bDetail = new BookingDetail
                            {
                                BookingId = booking.Id,
                                ServiceId = s.Id,
                                PriceAtBooking = s.Price
                            };
                            _context.BookingDetails.Add(bDetail);
                        }
                        await _context.SaveChangesAsync();

                        return RedirectToAction("Checkout", new { bookingId = booking.Id });
                    }
                }
            }
            
            // If we are here, something failed. Repopulate.
            model.UserVehicles = await _context.Vehicles.Where(v => v.UserId == userId).ToListAsync();
            model.AvailableServices = await _context.Services.ToListAsync();
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.Service)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            return View(booking);
        }
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Track(string reference)
        {
            if (string.IsNullOrEmpty(reference)) return View();

            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.Service)
                .Include(b => b.StaffAssignments).ThenInclude(sa => sa.Staff)
                .FirstOrDefaultAsync(b => b.BookingReference == reference);

            if (booking == null)
            {
                ViewBag.Error = "Invalid Booking Reference. Please check and try again.";
                return View();
            }

            return View(booking);
        }
    }
}
