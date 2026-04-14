using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitPM.Data;
using OrbitPM.Models;
using Microsoft.AspNetCore.Identity;

namespace OrbitPM.Controllers
{
    [Authorize(Roles = "ModuleLeader")]
    public class ModuleLeaderDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ModuleLeaderDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var ownerships = await _context.ProposalOwnerships
                .Include(po => po.Student)
                .Include(po => po.ProjectProposal)
                .ThenInclude(p => p!.ResearchArea)
                .Include(po => po.ProjectProposal!.MatchRecord)
                .ThenInclude(m => m!.Supervisor)
                .OrderByDescending(po => po.SubmittedAt)
                .ToListAsync();

            ViewBag.TotalProposals = ownerships.Count;
            ViewBag.MatchedProposals = ownerships.Count(o => o.ProjectProposal!.Status == ProjectStatus.Matched);
            ViewBag.PendingProposals = ownerships.Count(o => o.ProjectProposal!.Status == ProjectStatus.Pending);

            return View(ownerships);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceUnmatch(int proposalId)
        {
            var match = await _context.MatchRecords.FirstOrDefaultAsync(m => m.ProjectProposalId == proposalId);
            var proposal = await _context.ProjectProposals.FindAsync(proposalId);
            
            if (match != null && proposal != null)
            {
                _context.MatchRecords.Remove(match);
                proposal.Status = ProjectStatus.Pending;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Match revoked. Proposal returned to pending blind pool.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ResearchAreas()
        {
            var areas = await _context.ResearchAreas.ToListAsync();
            return View(areas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResearchArea(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                _context.ResearchAreas.Add(new ResearchArea { Name = name });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "New research domain added securely.";
            }
            return RedirectToAction(nameof(ResearchAreas));

        }

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(string fullName, string email, string role, string rawPassword)
        {
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(rawPassword))
            {
                var user = new ApplicationUser 
                { 
                    Id = Guid.NewGuid().ToString(),
                    FullName = fullName,
                    Email = email,
                    Role = role,
                    PasswordHash = string.Empty
                };

                var hasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = hasher.HashPassword(user, rawPassword);

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User {fullName} ({role}) created successfully.";
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
