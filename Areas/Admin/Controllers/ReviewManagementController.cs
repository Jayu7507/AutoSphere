using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSphere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReviewManagementController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Booking)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return View(reviews);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                review.IsApproved = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
