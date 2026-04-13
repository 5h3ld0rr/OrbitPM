using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitPM.Data;
using OrbitPM.Models;
using System.Security.Claims;

namespace OrbitPM.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class SupervisorDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SupervisorDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch pending proposals, inherently hiding student details since we never join the ownership table here!
            var availableProposals = await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == ProjectStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(availableProposals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExpressInterest(int proposalId)
        {
            var currentSupervisorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentSupervisorId)) return Challenge();

            var proposal = await _context.ProjectProposals.FindAsync(proposalId);
            if (proposal == null || proposal.Status != ProjectStatus.Pending)
            {
                return NotFound();
            }

            var match = new MatchRecord
            {
                ProjectProposalId = proposalId,
                SupervisorId = currentSupervisorId,
                MatchedAt = DateTime.UtcNow
            };

            proposal.Status = ProjectStatus.Matched;
            _context.MatchRecords.Add(match);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Match Confirmed for '{proposal.Title}'! Identity link has been successfully unlocked in the database.";
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ManageExpertise()
        {
            var supervisorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var allAreas = await _context.ResearchAreas.ToListAsync();
            var myAreas = await _context.SupervisorResearchAreas
                .Where(s => s.SupervisorId == supervisorId)
                .Select(s => s.ResearchAreaId)
                .ToListAsync();

            ViewBag.AllAreas = allAreas;
            ViewBag.MyAreas = myAreas;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageExpertise(List<int> selectedAreas)
        {
            var supervisorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // Remove existing
            var existing = await _context.SupervisorResearchAreas.Where(s => s.SupervisorId == supervisorId).ToListAsync();
            _context.SupervisorResearchAreas.RemoveRange(existing);
            
            // Add new
            if (selectedAreas != null && selectedAreas.Any())
            {
                foreach(var areaId in selectedAreas)
                {
                    _ = _context.SupervisorResearchAreas.Add(new SupervisorResearchArea { SupervisorId = supervisorId!, ResearchAreaId = areaId });
                }
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Research domains updated successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
