using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace AutoSphere.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServiceManagementController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.OrderByDescending(s => s.Id).ToListAsync();
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Service service, IFormFile? image1, IFormFile? image2, IFormFile? image3)
        {
            if (ModelState.IsValid)
            {
                if (image1 != null) service.ImageUrl1 = await SaveImage(image1);
                if (image2 != null) service.ImageUrl2 = await SaveImage(image2);
                if (image3 != null) service.ImageUrl3 = await SaveImage(image3);

                _context.Services.Add(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service added successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(service);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();
            
            return View(service);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Service service, IFormFile? image1, IFormFile? image2, IFormFile? image3)
        {
            if (ModelState.IsValid)
            {
                var existingService = await _context.Services.AsNoTracking().FirstOrDefaultAsync(s => s.Id == service.Id);
                if (existingService != null)
                {
                    service.ImageUrl1 = image1 != null ? await SaveImage(image1) : existingService.ImageUrl1;
                    service.ImageUrl2 = image2 != null ? await SaveImage(image2) : existingService.ImageUrl2;
                    service.ImageUrl3 = image3 != null ? await SaveImage(image3) : existingService.ImageUrl3;
                }

                _context.Entry(service).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(service);
        }

        private async Task<string> SaveImage(IFormFile file)
        {
            string uploadDir = Path.Combine(_webHostEnvironment.WebRootPath, "images", "services");
            if (!Directory.Exists(uploadDir))
            {
                Directory.CreateDirectory(uploadDir);
            }

            string fileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(file.FileName);
            string filePath = Path.Combine(uploadDir, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/images/services/" + fileName;
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.Services.FindAsync(id);
            if (service != null)
            {
                _context.Services.Remove(service);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Service deleted successfully!";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
