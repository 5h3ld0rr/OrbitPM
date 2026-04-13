using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitPM.Data;
using OrbitPM.Models;
using System.Security.Claims;

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

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomeDashboardViewModel();
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);

            if (User.IsInRole("Student"))
            {
                viewModel.DashboardTitle = "Student Command Center";
                viewModel.DashboardSubtitle = "Track your project submissions and supervisor matches securely.";
                
                var myProposals = await _context.ProposalOwnerships.Where(o => o.StudentId == userId).Select(o => o.ProjectProposalId).ToListAsync();
                
                viewModel.Stat1Label = "My Submissions";
                viewModel.Stat1Value = myProposals.Count.ToString();
                viewModel.Stat1Icon = "bi-journal-check";
                viewModel.Stat1Color = "text-primary";
                
                viewModel.Stat2Label = "My Matches";
                viewModel.Stat2Value = await _context.MatchRecords.CountAsync(m => myProposals.Contains(m.ProjectProposalId)) + "";
                viewModel.Stat2Icon = "bi-diagram-3-fill";
                viewModel.Stat2Color = "text-success";
                
                viewModel.Stat3Label = "Pending Review";
                viewModel.Stat3Value = await _context.ProjectProposals.CountAsync(p => myProposals.Contains(p.Id) && p.Status == ProjectStatus.Pending) + "";
                viewModel.Stat3Icon = "bi-hourglass-split";
                viewModel.Stat3Color = "text-warning";
                
                viewModel.Stat4Label = "Withdrawn";
                viewModel.Stat4Value = await _context.ProjectProposals.CountAsync(p => myProposals.Contains(p.Id) && p.Status == ProjectStatus.Withdrawn) + "";
                viewModel.Stat4Icon = "bi-archive-fill";
                viewModel.Stat4Color = "text-secondary";

                viewModel.TableTitle = "My Project Submissions";
                viewModel.ProposalsList = await _context.ProposalOwnerships
                    .Include(o => o.ProjectProposal)!.ThenInclude(p => p!.ResearchArea)
                    .Where(o => o.StudentId == userId && o.ProjectProposal != null)
                    .Select(o => o.ProjectProposal!)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            else if (User.IsInRole("Supervisor"))
            {
                viewModel.DashboardTitle = "Supervisor Control Panel";
                viewModel.DashboardSubtitle = "Manage your matched candidates and pending blind reviews.";
                
                viewModel.Stat1Label = "My Mentorships";
                viewModel.Stat1Value = await _context.MatchRecords.CountAsync(m => m.SupervisorId == userId) + "";
                viewModel.Stat1Icon = "bi-people-fill";
                viewModel.Stat1Color = "text-primary";
                
                viewModel.Stat2Label = "Total Queue";
                viewModel.Stat2Value = await _context.ProjectProposals.CountAsync(p => p.Status == ProjectStatus.Pending) + "";
                viewModel.Stat2Icon = "bi-inbox-fill";
                viewModel.Stat2Color = "text-info";
                
                viewModel.Stat3Label = "Available Students";
                viewModel.Stat3Value = await _context.Users.CountAsync(u => u.Role == "Student") + "";
                viewModel.Stat3Icon = "bi-mortarboard";
                viewModel.Stat3Color = "text-success";
                
                viewModel.Stat4Label = "My Tech Domains";
                viewModel.Stat4Value = await _context.SupervisorResearchAreas.CountAsync(s => s.SupervisorId == userId) + "";
                viewModel.Stat4Icon = "bi-tags-fill";
                viewModel.Stat4Color = "text-warning";

                viewModel.TableTitle = "My Recently Matched Projects";
                var matchedIds = await _context.MatchRecords.Where(m => m.SupervisorId == userId).Select(m => m.ProjectProposalId).ToListAsync();
                viewModel.ProposalsList = await _context.ProjectProposals
                    .Include(p => p.ResearchArea)
                    .Where(p => matchedIds.Contains(p.Id))
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            else
            {
                // Global / Module Leader / Logged out
                viewModel.DashboardTitle = "Network Overview";
                viewModel.DashboardSubtitle = "System pulse and active blind matchmaking statistics";
                
                viewModel.Stat1Label = "Proposals";
                viewModel.Stat1Value = await _context.ProjectProposals.CountAsync() + "";
                
                viewModel.Stat2Label = "Successful Matches";
                viewModel.Stat2Value = await _context.MatchRecords.CountAsync() + "";
                
                viewModel.Stat3Label = "Students";
                viewModel.Stat3Value = await _context.Users.CountAsync(u => u.Role == "Student") + "";
                
                viewModel.Stat4Label = "Supervisors";
                viewModel.Stat4Value = await _context.Users.CountAsync(u => u.Role == "Supervisor") + "";

                viewModel.TableTitle = "Live Radar: Recent Submissions";
                viewModel.ProposalsList = await _context.ProjectProposals
                    .Include(p => p.ResearchArea)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(5)
                    .ToListAsync();
            }
            
            return View(viewModel);
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
