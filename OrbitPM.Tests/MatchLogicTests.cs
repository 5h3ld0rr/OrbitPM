using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrbitPM.Controllers;
using OrbitPM.Data;
using OrbitPM.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace OrbitPM.Tests
{
    public class MatchLogicTests
    {
        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task ExpressInterest_ValidProposal_CreatesMatchRecordAndUpdatesStatus()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var supervisorId = "sup-123";
            
            var area = new ResearchArea { Id = 1, Name = "AI" };
            dbContext.ResearchAreas.Add(area);
            
            var proposal = new ProjectProposal 
            { 
                Id = 1, 
                Title = "Test Project", 
                Abstract = "Test Abstract", 
                ResearchAreaId = 1,
                Status = ProjectStatus.Pending 
            };
            dbContext.ProjectProposals.Add(proposal);
            await dbContext.SaveChangesAsync();

            var controller = new SupervisorDashboardController(dbContext)
            {
                TempData = new Mock<ITempDataDictionary>().Object
            };
            
            // Mocking the User identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, supervisorId),
                new Claim(ClaimTypes.Role, "Supervisor")
            }, "TestAuthentication"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Act
            var result = await controller.ExpressInterest(1);

            // Assert
            var updatedProposal = await dbContext.ProjectProposals.FindAsync(1);
            var matchRecord = await dbContext.MatchRecords.FirstOrDefaultAsync(m => m.ProjectProposalId == 1);

            Assert.Equal(ProjectStatus.Matched, updatedProposal?.Status);
            Assert.NotNull(matchRecord);
            Assert.Equal(supervisorId, matchRecord.SupervisorId);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task ExpressInterest_AlreadyMatchedProposal_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var supervisorId = "sup-123";
            
            var proposal = new ProjectProposal 
            { 
                Id = 2, 
                Title = "Already Matched", 
                Status = ProjectStatus.Matched,
                ResearchAreaId = 1
            };
            dbContext.ProjectProposals.Add(proposal);
            await dbContext.SaveChangesAsync();

            var controller = new SupervisorDashboardController(dbContext)
            {
                TempData = new Mock<ITempDataDictionary>().Object
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, supervisorId) }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.ExpressInterest(2);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ExpressInterest_NonExistentProposal_ReturnsNotFound()
        {
            // Arrange
            var dbContext = GetDatabaseContext();
            var controller = new SupervisorDashboardController(dbContext)
            {
                TempData = new Mock<ITempDataDictionary>().Object
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.NameIdentifier, "sup-1") }));
            controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

            // Act
            var result = await controller.ExpressInterest(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }
}
