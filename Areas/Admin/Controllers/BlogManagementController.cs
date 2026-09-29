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
    public class BlogManagementController : Controller
    {
        private readonly AppDbContext _context;

        public BlogManagementController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _context.BlogPosts.OrderByDescending(b => b.CreatedAt).ToListAsync();
            return View(posts);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(BlogPost post)
        {
            if (ModelState.IsValid)
            {
                post.Slug = post.Title.ToLower().Replace(" ", "-").Replace(":", "");
                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BlogPost post)
        {
            if (ModelState.IsValid)
            {
                post.Slug = post.Title.ToLower().Replace(" ", "-").Replace(":", "");
                _context.Entry(post).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post != null)
            {
                _context.BlogPosts.Remove(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
