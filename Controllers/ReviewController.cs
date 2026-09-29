using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoSphere.Controllers
{
    public class ReviewController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Submit(int bookingId, int rating, string comment)
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            // Check if booking belongs to user and is completed
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null || booking.UserId != userId || booking.Status != "Completed")
            {
                return BadRequest("Invalid booking for review.");
            }

            // Check if review already exists
            var existingReview = await _context.Reviews.FirstOrDefaultAsync(r => r.BookingId == bookingId);
            if (existingReview != null)
            {
                return Json(new { success = false, message = "You have already reviewed this service." });
            }

            var review = new Review
            {
                BookingId = bookingId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                IsApproved = false, // Pending Admin Approval
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Thank you! Your review is pending moderator approval." });
        }
    }
}
