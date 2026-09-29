using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using AutoSphere.Services;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSphere.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class BookingProcessingController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public BookingProcessingController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff == null) return NotFound();

            var assignments = await _context.StaffAssignments
                .Include(sa => sa.Booking!).ThenInclude(b => b.User)
                .Include(sa => sa.Booking!).ThenInclude(b => b.Vehicle)
                .Include(sa => sa.Booking!).ThenInclude(b => b.BookingDetails!).ThenInclude(bd => bd.Service)
                .Where(sa => sa.StaffId == staff.Id)
                .OrderByDescending(sa => sa.AssignedAt)
                .ToListAsync();

            return View(assignments);
        }

        public async Task<IActionResult> Detail(int id)
        {
            var assignment = await _context.StaffAssignments
                .Include(sa => sa.Booking!).ThenInclude(b => b.User)
                .Include(sa => sa.Booking!).ThenInclude(b => b.Vehicle)
                .Include(sa => sa.Booking!).ThenInclude(b => b.BookingDetails!).ThenInclude(bd => bd.Service)
                .Include(sa => sa.Staff)
                .FirstOrDefaultAsync(sa => sa.Id == id);

            if (assignment == null) return NotFound();

            // Security Check: Ensure this assignment belongs to the logged in staff
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (userIdStr != assignment.Staff?.UserId.ToString()) return Unauthorized();

            ViewBag.Parts = await _context.Parts.Where(p => p.IsActive && p.StockQuantity > 0).ToListAsync();

            return View(assignment);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProgress(int id, string status, int? partId, int partQty, decimal additionalCost, string serviceNotes)
        {
            var sa = await _context.StaffAssignments
                .Include(s => s.Booking)
                .Include(s => s.Staff)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (sa == null || sa.Booking == null) return NotFound();

            // Handle Part Usage
            if (partId.HasValue && partQty > 0)
            {
                var part = await _context.Parts.FindAsync(partId.Value);
                if (part != null && part.StockQuantity >= partQty)
                {
                    part.StockQuantity -= partQty;
                    
                    _context.BookingParts.Add(new BookingPart {
                        BookingId = sa.BookingId,
                        PartId = partId.Value,
                        Quantity = partQty,
                        UnitPriceAtBooking = part.Price
                    });

                    sa.PartsUsed += $" | {part.Name} (x{partQty})";
                    
                    decimal partTotal = part.Price * partQty;
                    sa.Booking.TotalAmount += partTotal;
                    sa.Booking.FinalAmount += partTotal;

                    // Check for Low Stock Notification
                    if (part.StockQuantity <= part.ThresholdLevel)
                    {
                        var admin = await _context.Users.FirstOrDefaultAsync(u => u.RoleId == 100); // Admin Role ID
                        if (admin != null)
                        {
                            _context.Notifications.Add(new Notification
                            {
                                UserId = admin.Id,
                                Title = "Low Stock Alert: " + part.Name,
                                Message = $"Stock for {part.Name} has fallen to {part.StockQuantity} units. Reorder recommended.",
                                CreatedAt = System.DateTime.UtcNow
                            });
                        }
                    }
                }
                else if (part != null)
                {
                    TempData["Error"] = $"Insufficient stock for {part.Name}.";
                    return RedirectToAction(nameof(Detail), new { id = sa.Id });
                }
            }

            // Update Assignment Info
            sa.AdditionalCost = additionalCost;
            sa.ServiceNotes = serviceNotes;

            // Update Booking Global Status
            sa.Booking.Status = status;
            
            // Sync Additional Charges to Booking
            sa.Booking.AdditionalCharges = additionalCost;
            
            // Recalculate Final Amount
            sa.Booking.FinalAmount = sa.Booking.TotalAmount + additionalCost - sa.Booking.DiscountAmount;

            // Trigger Notifications if Completed
            if (status == "Completed")
            {
                var booking = sa.Booking;
                var user = await _context.Users.FindAsync(booking.UserId);
                
                // In-App
                _context.Notifications.Add(new Notification
                {
                    UserId = booking.UserId,
                    Title = "Job Completed! ✅",
                    Message = $"Great news! Your service for {booking.Vehicle?.Brand} {booking.Vehicle?.ModelName} is finished and ready for pickup.",
                    CreatedAt = System.DateTime.UtcNow
                });

                // Email
                if (user != null)
                {
                    string emailBody = $@"
                        <div style='font-family: Arial; padding: 20px; background-color: #0B0F19; color: white;'>
                            <h2 style='color: #28a745;'>Job Completed!</h2>
                            <p>Hi {user.FullName},</p>
                            <p>We are happy to inform you that your service for <strong>{booking.Vehicle?.Brand} {booking.Vehicle?.ModelName}</strong> has been completed.</p>
                            <p>You can now pick up your vehicle from our center.</p>
                            <hr style='border-color: #333;' />
                            <p>Best Regards,<br/>AutoSphere Team</p>
                        </div>";
                    _ = _emailService.SendEmailAsync(user.Email, "Your Vehicle is Ready! - AutoSphere", emailBody);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Booking details updated successfully.";

            return RedirectToAction(nameof(Detail), new { id = sa.Id });
        }
    }
}
