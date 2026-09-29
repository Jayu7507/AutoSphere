using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace AutoSphere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var currentYear = now.Year;

            // Date Ranges for queries
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1);
            var prevMonthStart = monthStart.AddMonths(-1);
            var prevMonthEnd = monthStart;

            // KPIs for Current Month (Database side aggregation for performance)
            ViewBag.TotalRevenue = await _context.Bookings
                .Where(b => b.ScheduledDate >= monthStart && b.ScheduledDate < monthEnd && b.Status == "Completed")
                .SumAsync(b => b.FinalAmount * 1.18m);

            ViewBag.TotalJobs = await _context.Bookings
                .CountAsync(b => b.ScheduledDate >= monthStart && b.ScheduledDate < monthEnd);
            
            // Previous Month for Growth calculation
            var prevMonthRevenue = await _context.Bookings
                .Where(b => b.ScheduledDate >= prevMonthStart && b.ScheduledDate < prevMonthEnd && b.Status == "Completed")
                .SumAsync(b => b.FinalAmount * 1.18m);

            decimal growth = 0;
            if (prevMonthRevenue > 0)
                growth = ((ViewBag.TotalRevenue - prevMonthRevenue) / prevMonthRevenue) * 100;
            else if (ViewBag.TotalRevenue > 0)
                growth = 100;

            ViewBag.RevenueGrowth = growth;

            // Chart Data: Bookings by Status (All Time for better visualization)
            var statusCounts = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key ?? "Unknown", Count = g.Count() })
                .ToListAsync();

            ViewBag.StatusLabels = statusCounts.Select(s => s.Status).ToList();
            ViewBag.StatusData = statusCounts.Select(s => s.Count).ToList();

            // Low Stock Items Count
            ViewBag.LowStockCount = await _context.Parts.CountAsync(p => p.StockQuantity <= p.ThresholdLevel);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRevenueTrend()
        {
            // Last 6 months trend
            var trend = new List<object>();
            for(int i = 5; i >= 0; i--)
            {
                var target = DateTime.Now.AddMonths(-i);
                var revenue = await _context.Bookings
                    .Where(b => b.ScheduledDate.Month == target.Month && b.ScheduledDate.Year == target.Year && b.Status == "Completed")
                    .SumAsync(b => b.FinalAmount * 1.18m);
                
                trend.Add(new { month = target.ToString("MMM"), revenue });
            }
            return Json(trend);
        }
    }
}
