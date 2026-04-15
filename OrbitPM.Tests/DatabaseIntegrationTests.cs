using Microsoft.EntityFrameworkCore;
using OrbitPM.Data;
using OrbitPM.Models;

namespace OrbitPM.Tests
{
    public class DatabaseIntegrationTests
    {
        private ApplicationDbContext GetDatabaseContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "IntegrationTestDB_" + Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Database_ShouldPersistProjectWithResearchArea()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var area = new ResearchArea { Name = "Cloud Computing" };
            context.ResearchAreas.Add(area);
            await context.SaveChangesAsync();

            var proposal = new ProjectProposal
            {
                Title = "Cloud Migration Study",
                Abstract = "A detailed study on cloud migration strategies.",
                ResearchAreaId = area.Id,
                Status = ProjectStatus.Pending
            };

            // Act
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Assert
            var savedProposal = await context.ProjectProposals
                .Include(p => p.ResearchArea)
                .FirstOrDefaultAsync(p => p.Title == "Cloud Migration Study");

            Assert.NotNull(savedProposal);
            Assert.Equal("Cloud Computing", savedProposal.ResearchArea?.Name);
            Assert.Equal(ProjectStatus.Pending, savedProposal.Status);
        }

        [Fact]
        public async Task BlindMatch_StatePersistence_UnlocksOwnership()
        {
            // Arrange
            using var context = GetDatabaseContext();
            var studentId = "student-1";
            var area = new ResearchArea { Name = "Cybersecurity" };
            context.ResearchAreas.Add(area);
            await context.SaveChangesAsync();

            var proposal = new ProjectProposal
            {
                Title = "Zero Trust Architecture",
                Abstract = "Researching zero trust frameworks.",
                ResearchAreaId = area.Id
            };
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            // Blind ownership link
            var ownership = new ProposalOwnership
            {
                ProjectProposalId = proposal.Id,
                StudentId = studentId
            };
            context.ProposalOwnerships.Add(ownership);
            await context.SaveChangesAsync();

            // Act - Match occurs
            var match = new MatchRecord
            {
                ProjectProposalId = proposal.Id,
                SupervisorId = "supervisor-1",
                MatchedAt = DateTime.UtcNow
            };
            context.MatchRecords.Add(match);
            proposal.Status = ProjectStatus.Matched;
            await context.SaveChangesAsync();

            // Assert - Verify full graph can be traversed now
            var matchedData = await context.ProjectProposals
                .Include(p => p.Ownership)
                .Include(p => p.MatchRecord)
                .FirstOrDefaultAsync(p => p.Id == proposal.Id);

            Assert.NotNull(matchedData);
            Assert.NotNull(matchedData.Ownership);
            Assert.Equal(studentId, matchedData.Ownership.StudentId);
            Assert.NotNull(matchedData.MatchRecord);
            Assert.Equal("supervisor-1", matchedData.MatchRecord.SupervisorId);
        }
    }
}
