using System.ComponentModel.DataAnnotations;

namespace OrbitPM.Models
{
    public class ProjectProposal
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Abstract { get; set; } = string.Empty;

        public string TechnicalStack { get; set; } = string.Empty;

        // Exactly one primary research area (Edge Case requirement)
        public int ResearchAreaId { get; set; }
        public virtual ResearchArea? ResearchArea { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Critical Anonymity Layer:
        // By NOT storing the Student ID directly on this model, Supervisors loading
        // ProjectProposals have zero chance of seeing identity details until the 
        // MatchRecord exists and unlocks the join.
        public virtual ProposalOwnership? Ownership { get; set; }
        public virtual MatchRecord? MatchRecord { get; set; }
    }
}
