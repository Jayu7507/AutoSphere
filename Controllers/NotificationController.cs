using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoSphere.Controllers
{
    public class NotificationController : Controller
    {
        private readonly AppDbContext _context;

        public NotificationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            
            int userId = int.Parse(userIdStr);

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .Select(n => new { 
                    n.Id, 
                    n.Title, 
                    n.Message, 
                    n.IsRead, 
                    CreatedAt = n.CreatedAt.ToString("dd MMM, HH:mm") 
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

            return Json(new { notifications, unreadCount });
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }
    }
}
