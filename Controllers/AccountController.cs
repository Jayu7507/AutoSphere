using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoSphere.Data;
using AutoSphere.Models;
using AutoSphere.Models.ViewModels;
using AutoSphere.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoSphere.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                // Default Role is User (Assume RoleId 3 = User)
                var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
                if(userRole == null)
                {
                    userRole = new Role { Name = "User" };
                    _context.Roles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RoleId = userRole.Id,
                    IsEmailVerified = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate OTP
                await GenerateAndSendOTP(user.Email);
                
                return RedirectToAction("VerifyOTP", new { email = user.Email });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyOTP(string email)
        {
            return View(new VerifyOTPViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOTP(VerifyOTPViewModel model)
        {
            if (ModelState.IsValid)
            {
                var otpRecord = await _context.OTPVerifications
                    .Where(o => o.Email == model.Email && o.OTP == model.OTP && !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpRecord != null && otpRecord.ExpiryTime > DateTime.UtcNow)
                {
                    otpRecord.IsUsed = true;
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (user != null)
                    {
                        user.IsEmailVerified = true;
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Email verified successfully. You can now login.";
                        return RedirectToAction("Login");
                    }
                }
                ModelState.AddModelError("OTP", "Invalid or expired OTP.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin")) return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                if (User.IsInRole("Staff")) return RedirectToAction("Index", "Dashboard", new { area = "Staff" });
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                {
                    if (!user.IsEmailVerified)
                    {
                        await GenerateAndSendOTP(user.Email);
                        return RedirectToAction("VerifyOTP", new { email = user.Email });
                    }

                    if (user.Role == null)
                    {
                        ModelState.AddModelError(string.Empty, "User role is not properly configured. Please contact support.");
                        return View(model);
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.FullName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role.Name),
                        new Claim("UserId", user.Id.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Redirect based on role
                    if (user.Role.Name == "Admin") return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
                    if (user.Role.Name == "Staff") return RedirectToAction("Index", "Dashboard", new { area = "Staff" });
                    
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        private async Task GenerateAndSendOTP(string email)
        {
            string otp = new Random().Next(100000, 999999).ToString();
            var otpVerification = new OTPVerification
            {
                Email = email,
                OTP = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                Attempts = 0
            };
            _context.OTPVerifications.Add(otpVerification);
            await _context.SaveChangesAsync();

            await _emailService.SendOTPEmailAsync(email, otp);
        }

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    ModelState.AddModelError("Email", "If an account exists with this email, you will receive an OTP.");
                    return View(model);
                }

                await GenerateAndSendOTP(user.Email);
                TempData["Info"] = "Reset code sent! Please check your email.";
                return RedirectToAction("ResetPassword", new { email = user.Email });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPassword(string email)
        {
            return View(new ResetPasswordViewModel { Email = email });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var otpRecord = await _context.OTPVerifications
                    .Where(o => o.Email == model.Email && o.OTP == model.OTP && !o.IsUsed)
                    .OrderByDescending(o => o.CreatedAt)
                    .FirstOrDefaultAsync();

                if (otpRecord != null && otpRecord.ExpiryTime > DateTime.UtcNow)
                {
                    otpRecord.IsUsed = true;
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                    if (user != null)
                    {
                        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        await _context.SaveChangesAsync();
                        TempData["Success"] = "Password reset successfully. You can now login.";
                        return RedirectToAction("Login");
                    }
                }
                ModelState.AddModelError("OTP", "Invalid or expired OTP.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
