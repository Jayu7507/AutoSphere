using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using ClosedXML.Excel;

namespace AutoSphere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BookingManagementController : Controller
    {
        private readonly AppDbContext _context;

        public BookingManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User!)
                .Include(b => b.Vehicle!)
                .Include(b => b.BookingDetails!).ThenInclude(bd => bd.Service!)
                .Include(b => b.BookingParts!).ThenInclude(bp => bp.Part!)
                .Include(b => b.StaffAssignments!).ThenInclude(sa => sa.Staff!)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewBag.StaffList = await _context.Staffs.ToListAsync();
            ViewBag.PartsList = await _context.Parts.Where(p => p.IsActive && p.StockQuantity > 0).ToListAsync();

            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> AddPart(int bookingId, int partId, int quantity)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            var part = await _context.Parts.FindAsync(partId);

            if (booking != null && part != null)
            {
                if (part.StockQuantity < quantity)
                {
                    TempData["Error"] = $"Insufficient stock for {part.Name}. Available: {part.StockQuantity}";
                    return RedirectToAction(nameof(Index));
                }

                // Create BookingPart
                var bookingPart = new BookingPart
                {
                    BookingId = bookingId,
                    PartId = partId,
                    Quantity = quantity,
                    UnitPriceAtBooking = part.Price,
                    AddedAt = System.DateTime.UtcNow
                };

                // Deduct Stock
                part.StockQuantity -= quantity;

                // Update Booking Amount
                decimal partTotal = part.Price * quantity;
                booking.TotalAmount += partTotal;
                booking.FinalAmount += partTotal;

                _context.BookingParts.Add(bookingPart);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Added {quantity}x {part.Name} to Booking #{booking.BookingReference}. Total updated.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignStaff(int bookingId, int staffId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            var staff = await _context.Staffs.FindAsync(staffId);

            if (booking != null && staff != null)
            {
                // Remove existing assignments if any (assuming 1-to-1 for now for simplicity, 
                // though model supports M-to-1)
                var existing = _context.StaffAssignments.Where(sa => sa.BookingId == bookingId);
                _context.StaffAssignments.RemoveRange(existing);

                var assignment = new StaffAssignment
                {
                    BookingId = bookingId,
                    StaffId = staffId,
                    AssignedAt = System.DateTime.UtcNow
                };

                _context.StaffAssignments.Add(assignment);
                
                // Update booking status to Confirmed if it was Pending
                if(booking.Status == "Pending")
                {
                    booking.Status = "Confirmed";
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Staff {staff.Name} assigned to Booking #{booking.BookingReference}.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if(booking != null)
            {
                booking.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Booking #{booking.BookingReference} status updated to {status}.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportReport()
        {
            var bookings = await _context.Bookings
                .Include(b => b.User!)
                .Include(b => b.Vehicle!)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Bookings");
                var currentRow = 1;

                worksheet.Cell(currentRow, 1).Value = "Booking Ref";
                worksheet.Cell(currentRow, 2).Value = "Customer Name";
                worksheet.Cell(currentRow, 3).Value = "Vehicle Details";
                worksheet.Cell(currentRow, 4).Value = "Scheduled Date";
                worksheet.Cell(currentRow, 5).Value = "Status";
                worksheet.Cell(currentRow, 6).Value = "Final Amount";

                worksheet.Range("A1:F1").Style.Font.Bold = true;
                worksheet.Range("A1:F1").Style.Fill.BackgroundColor = XLColor.AirForceBlue;
                worksheet.Range("A1:F1").Style.Font.FontColor = XLColor.White;

                foreach (var b in bookings)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = b.BookingReference;
                    worksheet.Cell(currentRow, 2).Value = b.User?.FullName;
                    worksheet.Cell(currentRow, 3).Value = $"{b.Vehicle?.Brand} {b.Vehicle?.ModelName} ({b.Vehicle?.LicensePlate})";
                    worksheet.Cell(currentRow, 4).Value = b.ScheduledDate.ToString("dd MMM yyyy");
                    worksheet.Cell(currentRow, 5).Value = b.Status;
                    worksheet.Cell(currentRow, 6).Value = b.FinalAmount;
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "AutoSphere_Bookings_Report.xlsx");
                }
            }
        }
    }
}
