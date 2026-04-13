using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OrbitPM.Data;
using OrbitPM.Models;
using System.Security.Claims;

namespace OrbitPM.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentDashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Fetch the logged-in student's proposals.
            // Includes the MatchRecord and Supervisor to handle the "Identity Reveal"
            var myProposals = await _context.ProposalOwnerships
                .Include(po => po.ProjectProposal)
                .ThenInclude(p => p!.ResearchArea)
                .Include(po => po.ProjectProposal!.MatchRecord)
                .ThenInclude(m => m!.Supervisor) 
                .Where(po => po.StudentId == studentId)
                .ToListAsync();

            return View(myProposals);
        }

        public IActionResult CreateProposal()
        {
            ViewBag.ResearchAreas = new SelectList(_context.ResearchAreas, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProposal([Bind("Title,Abstract,TechnicalStack,ResearchAreaId")] ProjectProposal proposal)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                proposal.Status = ProjectStatus.Pending;
                
                var ownership = new ProposalOwnership
                {
                    ProjectProposal = proposal,
                    StudentId = studentId,
                    SubmittedAt = DateTime.UtcNow
                };

                _context.ProposalOwnerships.Add(ownership);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Proposal submitted securely! It is now blindly available in the supervisor queue.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ResearchAreas = new SelectList(_context.ResearchAreas, "Id", "Name", proposal.ResearchAreaId);
            return View(proposal);
        }
        
        [HttpGet]
        public async Task<IActionResult> EditProposal(int id)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ownership = await _context.ProposalOwnerships
                .Include(o => o.ProjectProposal)
                .FirstOrDefaultAsync(o => o.ProjectProposalId == id && o.StudentId == studentId);

            // Constraint: Cannot edit if already matched or doesn't belong to them
            if (ownership == null || ownership.ProjectProposal?.Status == ProjectStatus.Matched)
            {
                return NotFound();
            }

            ViewBag.ResearchAreas = new SelectList(_context.ResearchAreas, "Id", "Name", ownership.ProjectProposal.ResearchAreaId);
            return View(ownership.ProjectProposal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProposal(int id, [Bind("Id,Title,Abstract,TechnicalStack,ResearchAreaId")] ProjectProposal proposal)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ownership = await _context.ProposalOwnerships
                .Include(o => o.ProjectProposal)
                .FirstOrDefaultAsync(o => o.ProjectProposalId == id && o.StudentId == studentId);

            if (ownership == null || ownership.ProjectProposal?.Status == ProjectStatus.Matched)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                ownership.ProjectProposal!.Title = proposal.Title;
                ownership.ProjectProposal.Abstract = proposal.Abstract;
                ownership.ProjectProposal.TechnicalStack = proposal.TechnicalStack;
                ownership.ProjectProposal.ResearchAreaId = proposal.ResearchAreaId;
                
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Proposal updated securely.";
                return RedirectToAction(nameof(Index));
            }
            ViewBag.ResearchAreas = new SelectList(_context.ResearchAreas, "Id", "Name", proposal.ResearchAreaId);
            return View(proposal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> WithdrawProposal(int proposalId)
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ownership = await _context.ProposalOwnerships
                .Include(o => o.ProjectProposal)
                .FirstOrDefaultAsync(o => o.ProjectProposalId == proposalId && o.StudentId == studentId);

            // Constraint: Cannot withdraw if already matched or doesn't belong to them
            if (ownership == null || ownership.ProjectProposal?.Status == ProjectStatus.Matched)
            {
                return BadRequest("Cannot withdraw a matched or invalid proposal.");
            }

            ownership.ProjectProposal!.Status = ProjectStatus.Withdrawn;
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Your proposal has been successfully withdrawn. It is no longer visible to supervisors.";
            return RedirectToAction(nameof(Index));
        }
    }
}
