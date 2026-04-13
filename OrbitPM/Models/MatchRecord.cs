namespace OrbitPM.Models
{
    public class MatchRecord
    {
        public int Id { get; set; }

        public int ProjectProposalId { get; set; }
        public virtual ProjectProposal? ProjectProposal { get; set; }

        public string SupervisorId { get; set; } = string.Empty;
        public virtual ApplicationUser? Supervisor { get; set; }

        public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
    }
}
