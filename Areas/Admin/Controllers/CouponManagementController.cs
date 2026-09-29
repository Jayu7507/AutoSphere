using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using System.Threading.Tasks;

namespace AutoSphere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CouponManagementController : Controller
    {
        private readonly AppDbContext _context;

        public CouponManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _context.Coupons.ToListAsync();
            return View(coupons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Coupons.Add(coupon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon == null) return NotFound();
            return View(coupon);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                _context.Update(coupon);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await _context.Coupons.FindAsync(id);
            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
