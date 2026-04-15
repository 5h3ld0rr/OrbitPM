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
            var supervisorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            // 1. Get areas this supervisor is interested in
            var myAreaIds = await _context.SupervisorResearchAreas
                .Where(s => s.SupervisorId == supervisorId)
                .Select(s => s.ResearchAreaId)
                .ToListAsync();

            // 2. Fetch all pending proposals
            var availableProposals = await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == ProjectStatus.Pending)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            // 3. Count matches for the notification alert
            ViewBag.MatchCount = availableProposals.Count(p => myAreaIds.Contains(p.ResearchAreaId));
            ViewBag.MyAreaIds = myAreaIds;

            // 4. Statistics Calculation for the logged-in supervisor
            var myMatches = await _context.MatchRecords
                .Include(m => m.ProjectProposal)
                .Where(m => m.SupervisorId == supervisorId)
                .ToListAsync();

            ViewBag.Stats_Total = myMatches.Count;
            ViewBag.Stats_Approved = myMatches.Count(m => m.ProjectProposal?.Status == ProjectStatus.Approved);
            ViewBag.Stats_Pending = myMatches.Count(m => m.ProjectProposal?.Status == ProjectStatus.Matched); // Still in 'Matched' phase but not yet 'Approved'
            ViewBag.Stats_Rejected = myMatches.Count(m => m.ProjectProposal?.Status == ProjectStatus.Rejected);

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
            return RedirectToAction(nameof(ManageExpertise)); // Redirect back to manage instead of Index
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddResearchArea([FromForm] string name)
        {
            try 
            {
                if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

                var area = new ResearchArea { Name = name.Trim() };
                _context.ResearchAreas.Add(area);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Domain '{name}' added to system.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error adding domain: " + ex.Message;
            }
            return RedirectToAction(nameof(ManageExpertise));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResearchArea([FromForm] int id)
        {
            try
            {
                var area = await _context.ResearchAreas.FindAsync(id);
                if (area == null) return NotFound();

                _context.ResearchAreas.Remove(area);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Domain removed from system.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error removing domain. It may be linked to existing data.";
            }
            return RedirectToAction(nameof(ManageExpertise));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditResearchArea([FromForm] int id, [FromForm] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");

                var area = await _context.ResearchAreas.FindAsync(id);
                if (area == null) return NotFound();

                area.Name = name.Trim();
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Domain updated successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error updating domain: " + ex.Message;
            }
            return RedirectToAction(nameof(ManageExpertise));
        }
    }
}
