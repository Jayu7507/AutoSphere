using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using AutoSphere.Services;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace AutoSphere.Controllers
{
    public class PaymentController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IRazorpayService _razorpayService;
        private readonly IInvoiceService _invoiceService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public PaymentController(AppDbContext context, IRazorpayService razorpayService, IInvoiceService invoiceService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _razorpayService = razorpayService;
            _invoiceService = invoiceService;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Initiate(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            // Calculate total with GST
            decimal amountInRupees = (booking.FinalAmount * 1.18m);
            
            string razorpayOrderId;
            try
            {
                razorpayOrderId = _razorpayService.CreateOrder(amountInRupees, booking.BookingReference);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Payment Gateway Error: {ex.Message}. Please try Workshop Check-in or contact support." });
            }

            if (string.IsNullOrEmpty(razorpayOrderId))
            {
                return BadRequest(new { message = "Could not initialize payment with gateway. Please try Workshop Check-in or contact support." });
            }

            var payment = new Payment
            {
                BookingId = booking.Id,
                RazorpayOrderId = razorpayOrderId,
                Amount = amountInRupees,
                Status = "Pending",
                PaymentMethod = "Card/Digital",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return Json(new
            {
                orderId = razorpayOrderId,
                amount = (int)(amountInRupees * 100), // in paise
                key = _configuration["Razorpay:KeyId"],
                name = "AutoSphere",
                description = $"Booking Payment for {booking.BookingReference}",
                customerName = booking.User?.FullName ?? "Customer",
                customerEmail = booking.User?.Email ?? "",
                customerContact = booking.User?.Phone ?? ""
            });
        }

        [HttpPost]
        public async Task<IActionResult> Verify(string razorpay_payment_id, string razorpay_order_id, string razorpay_signature)
        {
            // Signature: orderId, paymentId, signature
            bool isValid = _razorpayService.VerifyPaymentSignature(razorpay_order_id, razorpay_payment_id, razorpay_signature);

            if (isValid)
            {
                var payment = await _context.Payments.Include(p => p.Booking).FirstOrDefaultAsync(p => p.RazorpayOrderId == razorpay_order_id);
                if (payment != null)
                {
                    payment.RazorpayPaymentId = razorpay_payment_id;
                    payment.RazorpaySignature = razorpay_signature;
                    payment.Status = "Success";

                    if (payment.Booking != null)
                    {
                        var booking = payment.Booking;
                        booking.Status = "Confirmed";
                        
                        // Create Invoice Record
                        var invoice = new Invoice
                        {
                            BookingId = booking.Id,
                            InvoiceNumber = "INV-" + booking.BookingReference,
                            GeneratedAt = DateTime.UtcNow
                        };
                        _context.Invoices.Add(invoice);

                        // Send In-App Notification
                        _context.Notifications.Add(new Notification
                        {
                            UserId = booking.UserId,
                            Title = "Booking Confirmed!",
                            Message = $"Your booking {booking.BookingReference} for {booking.ScheduledDate:dd MMM} has been confirmed.",
                            CreatedAt = DateTime.UtcNow
                        });

                        // Send Email Confirmation (Async background-ish)
                        var user = await _context.Users.FindAsync(booking.UserId);
                        if (user != null)
                        {
                            string emailBody = $@"
                                <div style='font-family: Arial; padding: 20px; background-color: #0B0F19; color: white;'>
                                    <h2 style='color: #0F52BA;'>Booking Confirmed!</h2>
                                    <p>Hi {user?.FullName ?? "Customer"},</p>
                                    <p>Your payment for booking <strong>{booking.BookingReference}</strong> was successful.</p>
                                    <p><strong>Scheduled Date:</strong> {booking.ScheduledDate:dd MMM yyyy}</p>
                                    <p>Thank you for choosing AutoSphere!</p>
                                </div>";
                            if (user!.Email != null)
                            {
                                _ = _emailService.SendEmailAsync(user.Email, "Booking Confirmed - AutoSphere", emailBody);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, bookingId = payment.BookingId });
                }
            }

            return Json(new { success = false, message = "Payment verification failed" });
        }

        [HttpPost]
        public async Task<IActionResult> PayLater(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return Json(new { success = false, message = "Booking not found" });

            // 1. Create Payment Record for Pay Later
            var payment = new Payment
            {
                BookingId = booking.Id,
                RazorpayOrderId = "PAY_LATER_" + Guid.NewGuid().ToString().Substring(0, 8),
                Amount = booking.FinalAmount * 1.18m,
                Status = "Pending", // Status is pending until they pay in shop
                PaymentMethod = "Workshop/COD",
                CreatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);
            
            // 2. Confirm Booking
            booking.Status = "Confirmed";

            // 3. Create Invoice Record
            var invoice = new Invoice
            {
                BookingId = booking.Id,
                InvoiceNumber = "INV-" + booking.BookingReference,
                GeneratedAt = DateTime.UtcNow
            };
            _context.Invoices.Add(invoice);

            // 4. Send Notification
            _context.Notifications.Add(new Notification
            {
                UserId = booking.UserId,
                Title = "Booking Secured (Pay Later)",
                Message = $"Your booking {booking.BookingReference} is confirmed. Please pay ₹{payment.Amount:0.00} at the workshop.",
                CreatedAt = DateTime.UtcNow
            });

            // 5. Send Email Confirmation
            if (booking.User != null)
            {
                string emailBody = $@"
                    <div style='font-family: Arial; padding: 20px; background-color: #0B0F19; color: white;'>
                        <h2 style='color: #FF6B00;'>Booking Secured - Pay at Workshop</h2>
                        <p>Hi {booking.User.FullName},</p>
                        <p>Your booking <strong>{booking.BookingReference}</strong> has been secured.</p>
                        <p><strong>Payment Mode:</strong> Pay at Workshop (Cash/Card)</p>
                        <p><strong>Amount Due:</strong> ₹{payment.Amount:0.00}</p>
                        <p><strong>Scheduled Date:</strong> {booking.ScheduledDate:dd MMM yyyy}</p>
                        <p>See you at the workshop!</p>
                    </div>";
                _ = _emailService.SendEmailAsync(booking.User.Email, "Booking Secured - AutoSphere", emailBody);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        public async Task<IActionResult> Success(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.BookingDetails).ThenInclude(d => d.Service)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            return View(booking);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadInvoice(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Vehicle)
                .Include(b => b.BookingDetails).ThenInclude(d => d.Service)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return NotFound();

            var pdfBytes = await _invoiceService.GenerateInvoicePdfAsync(booking);
            return File(pdfBytes, "application/pdf", $"Invoice_{booking.BookingReference}.pdf");
        }
    }
}
