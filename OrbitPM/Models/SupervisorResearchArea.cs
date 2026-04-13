namespace OrbitPM.Models
{
    public class SupervisorResearchArea
    {
        public int Id { get; set; }

        public string SupervisorId { get; set; } = string.Empty;
        public virtual ApplicationUser? Supervisor { get; set; }

        public int ResearchAreaId { get; set; }
        public virtual ResearchArea? ResearchArea { get; set; }
    }
}
