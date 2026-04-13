namespace OrbitPM.Models
{
    public class ProposalOwnership
    {
        public int Id { get; set; }

        public int ProjectProposalId { get; set; }
        public virtual ProjectProposal? ProjectProposal { get; set; }

        public string StudentId { get; set; } = string.Empty;
        public virtual ApplicationUser? Student { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
