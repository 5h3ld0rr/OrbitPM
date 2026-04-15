using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrbitPM.Data;
using OrbitPM.Models;

using Microsoft.AspNetCore.Authorization;

namespace OrbitPM.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        public IActionResult Index()
        {
            if (User.IsInRole("Student"))
            {
                return RedirectToAction("Index", "StudentDashboard");
            }
            else if (User.IsInRole("Supervisor"))
            {
                return RedirectToAction("Index", "SupervisorDashboard");
            }
            else if (User.IsInRole("ModuleLeader"))
            {
                return RedirectToAction("Index", "ModuleLeaderDashboard");
            }
            
            return RedirectToAction("Login", "Account");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
