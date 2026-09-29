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
    public class InventoryManagementController : Controller
    {
        private readonly AppDbContext _context;

        public InventoryManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var parts = await _context.Parts.ToListAsync();
            return View(parts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Part part)
        {
            if (ModelState.IsValid)
            {
                _context.Parts.Add(part);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(part);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part == null) return NotFound();
            return View(part);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Part part)
        {
            if (ModelState.IsValid)
            {
                _context.Update(part);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(part);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var part = await _context.Parts.FindAsync(id);
            if (part != null)
            {
                // Soft Delete: Keep the part in DB for historical booking records, 
                // but mark as inactive so it doesn't show in lists.
                part.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Part '{part.Name}' has been deactivated.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
