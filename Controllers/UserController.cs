using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using AutoSphere.Services;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoSphere.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IMaintenanceService _maintenanceService;

        public UserController(AppDbContext context, IMaintenanceService maintenanceService)
        {
            _context = context;
            _maintenanceService = maintenanceService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var bookings = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.Service)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            var vehicles = await _context.Vehicles
                .Where(v => v.UserId == userId)
                .ToListAsync();

            ViewBag.Vehicles = vehicles;
            
            // AI-Like Predictive Logic via MaintenanceService
            var insight = _maintenanceService.GetNextServiceInsight(null, bookings); // Passing null for user as it's not strictly needed for current logic

            ViewBag.NextServiceDate = insight.PredictiveDate;
            ViewBag.RecommendedService = insight.Recommendation;

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> AddVehicle(Vehicle vehicle)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            vehicle.UserId = int.Parse(userIdStr);
            
            if (ModelState.IsValid)
            {
                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle added successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to add vehicle. Please check your inputs.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.UserId == userId);
            if (vehicle != null)
            {
                _context.Vehicles.Remove(vehicle);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Vehicle removed.";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string fullName, string phone)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.FullName = fullName;
                user.Phone = phone;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully!";
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                if (BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Password changed successfully!";
                }
                else
                {
                    TempData["Error"] = "Current password is incorrect.";
                }
            }

            return RedirectToAction(nameof(Dashboard));
        }

        [HttpGet]
        public async Task<IActionResult> EditBooking(int id)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var booking = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.BookingDetails).ThenInclude(bd => bd.Service)
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);

            if (booking == null) return NotFound();
            if (booking.Status != "Pending")
            {
                TempData["Error"] = "Only pending bookings can be modified.";
                return RedirectToAction(nameof(Dashboard));
            }

            var model = new Models.ViewModels.BookingEditViewModel
            {
                Id = booking.Id,
                BookingReference = booking.BookingReference,
                ScheduledDate = booking.ScheduledDate,
                AdditionalNotes = booking.AdditionalNotes,
                IsPickupDelivery = booking.IsPickupDelivery,
                PickupAddress = booking.PickupAddress,
                DeliveryAddress = booking.DeliveryAddress,
                VehicleInfo = $"{booking.Vehicle?.Brand} {booking.Vehicle?.ModelName} ({booking.Vehicle?.LicensePlate})",
                ServicesInfo = string.Join(", ", booking.BookingDetails.Select(bd => bd.Service?.Name))
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditBooking(Models.ViewModels.BookingEditViewModel model)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            if (ModelState.IsValid)
            {
                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == model.Id && b.UserId == userId);
                if (booking != null && booking.Status == "Pending")
                {
                    booking.ScheduledDate = model.ScheduledDate;
                    booking.AdditionalNotes = model.AdditionalNotes;
                    booking.IsPickupDelivery = model.IsPickupDelivery;
                    booking.PickupAddress = model.PickupAddress;
                    booking.DeliveryAddress = model.DeliveryAddress;

                    await _context.SaveChangesAsync();
                    TempData["Success"] = $"Booking #{booking.BookingReference} updated successfully.";
                    return RedirectToAction(nameof(Dashboard));
                }
            }

            TempData["Error"] = "Failed to update booking. Please try again.";
            return View(model);
        }
    }
}
