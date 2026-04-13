using Microsoft.EntityFrameworkCore;
using OrbitPM.Models;

namespace OrbitPM.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<ProjectProposal> ProjectProposals { get; set; }
        public DbSet<ProposalOwnership> ProposalOwnerships { get; set; }
        public DbSet<MatchRecord> MatchRecords { get; set; }
        public DbSet<ResearchArea> ResearchAreas { get; set; }
        public DbSet<SupervisorResearchArea> SupervisorResearchAreas { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Entity Framework Models for the Blind-Match constraints
            builder.Entity<ProjectProposal>()
                .HasOne(p => p.Ownership)
                .WithOne(o => o.ProjectProposal)
                .HasForeignKey<ProposalOwnership>(o => o.ProjectProposalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectProposal>()
                .HasOne(p => p.MatchRecord)
                .WithOne(m => m.ProjectProposal)
                .HasForeignKey<MatchRecord>(m => m.ProjectProposalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SupervisorResearchArea>()
                .HasIndex(sra => new { sra.SupervisorId, sra.ResearchAreaId })
                .IsUnique();
        }
    }
}
