using System.ComponentModel.DataAnnotations;

namespace OrbitPM.Models
{
    public class ApplicationUser 
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "Student";
        
        public string? FullName { get; set; }
        
        // Navigation properties
        public virtual ICollection<ProposalOwnership> ProposalOwnerships { get; set; } = new List<ProposalOwnership>();
        public virtual ICollection<MatchRecord> Matches { get; set; } = new List<MatchRecord>();
        public virtual ICollection<SupervisorResearchArea> SupervisorResearchAreas { get; set; } = new List<SupervisorResearchArea>();
    }
}
