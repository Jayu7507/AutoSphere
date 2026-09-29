using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using System;
using System.Threading.Tasks;

namespace AutoSphere.Controllers
{
    public class CouponController : Controller
    {
        private readonly AppDbContext _context;

        public CouponController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Validate(string code, decimal currentTotal)
        {
            if (string.IsNullOrEmpty(code))
                return Json(new { success = false, message = "Coupon code is required" });

            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code == code && c.IsActive && c.ExpiryDate > DateTime.Now);

            if (coupon == null)
            {
                return Json(new { success = false, message = "Invalid or expired coupon code" });
            }

            decimal discount = (currentTotal * coupon.DiscountPercentage) / 100;
            if (discount > coupon.MaxDiscountAmount)
            {
                discount = coupon.MaxDiscountAmount;
            }

            return Json(new 
            { 
                success = true, 
                discountAmount = discount, 
                newTotal = currentTotal - discount,
                message = $"Coupon applied! You saved ₹{discount:N2}"
            });
        }
    }
}
