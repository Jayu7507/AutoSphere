using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSphere.Controllers
{
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? searchKeyword = null)
        {
            var servicesQuery = _context.Services.AsQueryable();

            if (!string.IsNullOrEmpty(searchKeyword))
            {
                servicesQuery = servicesQuery.Where(s => s.Name.Contains(searchKeyword) || (s.Description != null && s.Description.Contains(searchKeyword)));
            }

            var services = await servicesQuery.ToListAsync();
            return View(services);
        }
    }
}
