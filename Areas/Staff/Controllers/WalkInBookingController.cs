using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using AutoSphere.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSphere.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff,Admin")]
    public class WalkInBookingController : Controller
    {
        private readonly AppDbContext _context;

        public WalkInBookingController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Staff/WalkInBooking
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // GET: Staff/WalkInBooking/Search
        [HttpGet]
        public async Task<IActionResult> Search(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return RedirectToAction(nameof(Index));

            var user = await _context.Users
                .Include(u => u.Vehicles)
                .FirstOrDefaultAsync(u => u.Phone == phone);

            return RedirectToAction(nameof(Create), new { phone = phone });
        }

        // GET: Staff/WalkInBooking/Create
        [HttpGet]
        public async Task<IActionResult> Create(string phone)
        {
            var user = await _context.Users
                .Include(u => u.Vehicles)
                .FirstOrDefaultAsync(u => u.Phone == phone);

            var model = new WalkInBookingViewModel
            {
                Phone = phone,
                FullName = user?.FullName ?? "",
                Email = user?.Email ?? "",
                ExistingVehicles = user?.Vehicles.ToList() ?? new List<Vehicle>(),
                AvailableServices = await _context.Services.ToListAsync(),
                ScheduledDate = DateTime.Today
            };

            return View(model);
        }

        // POST: Staff/WalkInBooking/Create
        [HttpPost]
        public async Task<IActionResult> Create(WalkInBookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Handle User
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Phone == model.Phone);
                    if (user == null)
                    {
                        // Create new User for walk-in
                        var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                        user = new User
                        {
                            FullName = model.FullName,
                            Phone = model.Phone,
                            Email = string.IsNullOrEmpty(model.Email) ? $"{model.Phone}@walkin.autosphere.com" : model.Email,
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Phone), // Default password is phone
                            RoleId = userRole?.Id ?? 3,
                            IsEmailVerified = true, // Trusted as they are in-person
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.Users.Add(user);
                        await _context.SaveChangesAsync();
                    }

                    // 2. Handle Vehicle
                    int vehicleId = 0;
                    if (model.SelectedVehicleId.HasValue && model.SelectedVehicleId.Value > 0)
                    {
                        vehicleId = model.SelectedVehicleId.Value;
                    }
                    else
                    {
                        var vehicle = new Vehicle
                        {
                            UserId = user.Id,
                            Type = model.VehicleType,
                            Brand = model.Brand,
                            ModelName = model.ModelName,
                            LicensePlate = model.LicensePlate.ToUpper()
                        };
                        _context.Vehicles.Add(vehicle);
                        await _context.SaveChangesAsync();
                        vehicleId = vehicle.Id;
                    }

                    // 3. Handle Services & Totals
                    var services = await _context.Services
                        .Where(s => model.SelectedServiceIds.Contains(s.Id))
                        .ToListAsync();

                    if (!services.Any())
                    {
                        ModelState.AddModelError("", "Please select at least one service.");
                        model.AvailableServices = await _context.Services.ToListAsync();
                        model.ExistingVehicles = user?.Vehicles.ToList() ?? new List<Vehicle>();
                        return View(model);
                    }

                    decimal total = services.Sum(s => s.Price);

                    // 4. Create Booking
                    string reference;
                    do
                    {
                        reference = "WIK" + new Random().Next(10000, 99999).ToString();
                    } while (await _context.Bookings.AnyAsync(b => b.BookingReference == reference));

                    var booking = new Booking
                    {
                        BookingReference = reference,
                        UserId = user.Id,
                        VehicleId = vehicleId,
                        ScheduledDate = model.ScheduledDate,
                        Status = "Confirmed", // Instant confirmation for walk-ins
                        TotalAmount = total,
                        DiscountAmount = 0,
                        FinalAmount = total,
                        AdditionalNotes = model.AdditionalNotes
                    };
                    _context.Bookings.Add(booking);
                    await _context.SaveChangesAsync();

                    foreach (var s in services)
                    {
                        _context.BookingDetails.Add(new BookingDetail
                        {
                            BookingId = booking.Id,
                            ServiceId = s.Id,
                            PriceAtBooking = s.Price
                        });
                    }
                    await _context.SaveChangesAsync();

                    // 5. Auto-Assignment
                    if (model.AutoAssignToMe)
                    {
                        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
                        if (!string.IsNullOrEmpty(userIdStr))
                        {
                            int staffUserId = int.Parse(userIdStr);
                            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserId == staffUserId);
                            if (staff != null)
                            {
                                _context.StaffAssignments.Add(new StaffAssignment
                                {
                                    BookingId = booking.Id,
                                    StaffId = staff.Id,
                                    AssignedAt = DateTime.UtcNow
                                });
                                await _context.SaveChangesAsync();
                            }
                        }
                    }

                    await transaction.CommitAsync();
                    TempData["Success"] = $"Walk-in booking {reference} created successfully!";
                    return RedirectToAction("Index", "Dashboard", new { area = "Staff" });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", "Error creating walk-in booking: " + ex.Message);
                }
            }

            model.AvailableServices = await _context.Services.ToListAsync();
            return View(model);
        }
    }
}
