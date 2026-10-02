using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoSphere.Models;
using AutoSphere.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace AutoSphere.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var approvedReviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(6)
            .ToListAsync();

        var ratings = await _context.Reviews
            .Where(r => r.IsApproved)
            .Select(r => r.Rating)
            .ToListAsync();

        double averageRating = ratings.Any() ? ratings.Average() : 5.0;
        int reviewCount = ratings.Count;

        ViewBag.ApprovedReviews = approvedReviews;
        ViewBag.AverageRating = averageRating.ToString("F1");
        ViewBag.ReviewCount = reviewCount;

        var latestPosts = await _context.BlogPosts
            .Where(b => b.IsPublished)
            .OrderByDescending(b => b.CreatedAt)
            .Take(2)
            .ToListAsync();
        
        ViewBag.LatestPosts = latestPosts;

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            return Json(new { success = false, message = "Please provide a valid email address." });
        }

        var existing = await _context.NewsletterSubscribers.AnyAsync(s => s.Email == email);
        if (existing)
        {
            return Json(new { success = false, message = "You are already subscribed to the Insider Circle!" });
        }

        var subscriber = new NewsletterSubscriber { Email = email };
        _context.NewsletterSubscribers.Add(subscriber);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Welcome to the Inner Circle! You will receive our next intelligence update soon." });
    }

    [HttpPost]
    public async Task<IActionResult> Contact(ContactMessage model)
    {
        if (ModelState.IsValid)
        {
            model.CreatedAt = DateTime.UtcNow;
            model.IsResolved = false;
            _context.ContactMessages.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thank you! Your message has been received. Our concierge team will reach out shortly." });
        }
        return Json(new { success = false, message = "Please fill in all required fields." });
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
