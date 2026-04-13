namespace OrbitPM.Models
{
    public class HomeDashboardViewModel
    {
        public string DashboardTitle { get; set; } = "Network Overview";
        public string DashboardSubtitle { get; set; } = "System pulse and active blind matchmaking statistics";
        
        public string Stat1Label { get; set; } = "";
        public string Stat1Value { get; set; } = "";
        public string Stat1Icon { get; set; } = "bi-journal-text";
        public string Stat1Color { get; set; } = "text-primary";
        
        public string Stat2Label { get; set; } = "";
        public string Stat2Value { get; set; } = "";
        public string Stat2Icon { get; set; } = "bi-diagram-3-fill";
        public string Stat2Color { get; set; } = "text-success";
        
        public string Stat3Label { get; set; } = "";
        public string Stat3Value { get; set; } = "";
        public string Stat3Icon { get; set; } = "bi-mortarboard";
        public string Stat3Color { get; set; } = "text-info";
        
        public string Stat4Label { get; set; } = "";
        public string Stat4Value { get; set; } = "";
        public string Stat4Icon { get; set; } = "bi-person-badge";
        public string Stat4Color { get; set; } = "text-warning";

        public string TableTitle { get; set; } = "Live Radar: Recent Submissions";
        public List<ProjectProposal> ProposalsList { get; set; } = new List<ProjectProposal>();
    }
}
