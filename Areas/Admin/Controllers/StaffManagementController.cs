using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AutoSphere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StaffManagementController : Controller
    {
        private readonly AppDbContext _context;

        public StaffManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var staffList = await _context.Staffs
                .Include(s => s.User)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(staffList);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(AutoSphere.Models.Staff staff, string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and Password are required for staff login.");
            }

            if (ModelState.IsValid)
            {
                // 1. Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("email", "A user with this email already exists.");
                    return View(staff);
                }

                // 2. Get Staff Role
                var staffRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Staff");
                if (staffRole == null)
                {
                    TempData["Error"] = "Staff role not found in system.";
                    return RedirectToAction(nameof(Index));
                }

                // 3. Create User Account
                var newUser = new User
                {
                    FullName = staff.Name,
                    Email = email,
                    Phone = staff.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    RoleId = staffRole.Id,
                    IsEmailVerified = true // Admin-added accounts are pre-verified
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                // 4. Create Staff Link
                staff.UserId = newUser.Id;
                _context.Staffs.Add(staff);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Staff member and login account created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null) return NotFound();
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(AutoSphere.Models.Staff staff)
        {
            if (ModelState.IsValid)
            {
                _context.Update(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Staff details updated.";
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var staff = await _context.Staffs
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (staff != null)
            {
                // Remove the associated User record to prevent 'Ghost Users'
                if (staff.User != null)
                {
                    _context.Users.Remove(staff.User);
                }

                _context.Staffs.Remove(staff);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Staff member and their login account removed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
