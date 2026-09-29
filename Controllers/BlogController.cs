using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using System.Threading.Tasks;

namespace AutoSphere.Controllers
{
    public class BlogController : Controller
    {
        private readonly AppDbContext _context;

        public BlogController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.BlogPosts
                .Where(b => b.IsPublished)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return RedirectToAction(nameof(Index));

            var post = await _context.BlogPosts
                .FirstOrDefaultAsync(b => b.Slug == slug && b.IsPublished);

            if (post == null) return NotFound();

            return View(post);
        }
    }
}
