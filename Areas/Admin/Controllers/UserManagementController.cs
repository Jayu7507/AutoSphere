using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSphere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;

        public UserManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Role)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }
    
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.Include(u => u.Role!).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null || user.Role?.Name == "Admin") return RedirectToAction(nameof(Index));
            
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(user);
        }
    
        [HttpPost]
        public async Task<IActionResult> Edit(int id, int RoleId)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.RoleId = RoleId;
                await _context.SaveChangesAsync();
                TempData["Success"] = "User role updated successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // Simple hard delete for demonstration
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                // Prevent deleting self or other admins easily
                if(user.Role?.Name != "Admin") 
                {
                    _context.Users.Remove(user);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "User deleted successfully.";
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
