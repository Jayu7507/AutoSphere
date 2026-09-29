using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AutoSphere.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Authorize(Roles = "Staff")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            int userId = int.Parse(userIdStr);

            var staff = await _context.Staffs
                .Include(s => s.Assignments).ThenInclude(a => a.Booking!).ThenInclude(b => b.User)
                .Include(s => s.Assignments).ThenInclude(a => a.Booking!).ThenInclude(b => b.Vehicle)
                .Include(s => s.Assignments).ThenInclude(a => a.Booking!).ThenInclude(b => b.BookingDetails!).ThenInclude(bd => bd.Service)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (staff == null)
            {
                // This user is a Staff in Roles but not registered in Staffs table
                return View("NotRegistered");
            }

            return View(staff);
        }
    }
}
