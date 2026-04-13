using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitPM.Data;
using OrbitPM.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity; // Still valid for barebones PasswordHasher utility without full Identity setup

namespace OrbitPM.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password, string returnUrl = "/")
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null)
            {
                var hasher = new PasswordHasher<ApplicationUser>();
                var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
                
                if (result == PasswordVerificationResult.Success)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.FullName ?? user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
                    
                    return LocalRedirect(returnUrl ?? "/");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
