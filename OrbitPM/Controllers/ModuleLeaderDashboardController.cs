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
                .Where(o => o.ProjectProposal!.Status != ProjectStatus.Withdrawn)
                .OrderByDescending(po => po.SubmittedAt)
                .ToListAsync();

            ViewBag.TotalProposals = ownerships.Count;
            ViewBag.AllocatedProposals = ownerships.Count(o => o.ProjectProposal!.Status == ProjectStatus.Approved);
            ViewBag.PendingApproval = ownerships.Count(o => o.ProjectProposal!.Status == ProjectStatus.Matched);
            ViewBag.PendingSubmissions = ownerships.Count(o => o.ProjectProposal!.Status == ProjectStatus.Pending);

            return View(ownerships);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveMatch(int proposalId)
        {
            var proposal = await _context.ProjectProposals.FindAsync(proposalId);
            if (proposal != null && proposal.Status == ProjectStatus.Matched)
            {
                proposal.Status = ProjectStatus.Approved;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Match finalized. Allocation record secured.";
            }
            return RedirectToAction(nameof(Index));
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
            var areas = await _context.ResearchAreas.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Id).ToListAsync();
            
            // Auto-initialize orders if they are all 0 (first-run optimization)
            if (areas.Any() && areas.All(a => a.DisplayOrder == 0))
            {
                for (int i = 0; i < areas.Count; i++)
                {
                    areas[i].DisplayOrder = i + 1;
                }
                await _context.SaveChangesAsync();
                areas = await _context.ResearchAreas.OrderBy(a => a.DisplayOrder).ToListAsync();
            }
            
            return View(areas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateResearchArea(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var maxOrder = await _context.ResearchAreas.MaxAsync(a => (int?)a.DisplayOrder) ?? 0;
                _context.ResearchAreas.Add(new ResearchArea { Name = name, DisplayOrder = maxOrder + 1 });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Domain '{name}' added to system registry.";
            }
            return RedirectToAction(nameof(ResearchAreas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditResearchArea(int id, string name)
        {
            var area = await _context.ResearchAreas.FindAsync(id);
            if (area != null && !string.IsNullOrWhiteSpace(name))
            {
                area.Name = name;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Domain name updated successfully.";
            }
            return RedirectToAction(nameof(ResearchAreas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteResearchArea(int id)
        {
            var area = await _context.ResearchAreas.Include(a => a.Proposals).FirstOrDefaultAsync(a => a.Id == id);
            if (area != null)
            {
                if (area.Proposals.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete domain: it is currently linked to active project proposals.";
                    return RedirectToAction(nameof(ResearchAreas));
                }

                _context.ResearchAreas.Remove(area);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Domain removed from system registry.";
            }
            return RedirectToAction(nameof(ResearchAreas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReorderResearchArea(int id, string direction)
        {
            var areas = await _context.ResearchAreas.OrderBy(a => a.DisplayOrder).ToListAsync();
            var area = areas.FirstOrDefault(a => a.Id == id);
            
            if (area != null)
            {
                int currentIndex = areas.IndexOf(area);
                if (direction == "up" && currentIndex > 0)
                {
                    var other = areas[currentIndex - 1];
                    (area.DisplayOrder, other.DisplayOrder) = (other.DisplayOrder, area.DisplayOrder);
                }
                else if (direction == "down" && currentIndex < areas.Count - 1)
                {
                    var other = areas[currentIndex + 1];
                    (area.DisplayOrder, other.DisplayOrder) = (other.DisplayOrder, area.DisplayOrder);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ResearchAreas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDomainOrder([FromBody] List<int> sortedIds)
        {
            if (sortedIds == null || !sortedIds.Any()) return BadRequest();

            var areas = await _context.ResearchAreas.ToListAsync();
            
            for (int i = 0; i < sortedIds.Count; i++)
            {
                var area = areas.FirstOrDefault(a => a.Id == sortedIds[i]);
                if (area != null)
                {
                    area.DisplayOrder = i + 1;
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
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
            if (role == "ModuleLeader")
            {
                TempData["ErrorMessage"] = "You do not have permission to provision additional administrative accounts.";
                return RedirectToAction(nameof(Users));
            }

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (user != null)
            {
                if (user.Id == currentUserId)
                {
                    TempData["ErrorMessage"] = "You cannot delete your own administrative account.";
                    return RedirectToAction(nameof(Users));
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"User {user.FullName} has been permanently removed from the system.";
            }
            return RedirectToAction(nameof(Users));
        }
    }
}
