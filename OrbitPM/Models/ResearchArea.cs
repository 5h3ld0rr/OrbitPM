using System.ComponentModel.DataAnnotations;

namespace OrbitPM.Models
{
    public class ResearchArea
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<ProjectProposal> Proposals { get; set; } = new List<ProjectProposal>();
        public virtual ICollection<SupervisorResearchArea> SupervisorResearchAreas { get; set; } = new List<SupervisorResearchArea>();
    }
}
