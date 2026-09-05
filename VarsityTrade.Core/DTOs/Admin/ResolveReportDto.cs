namespace VarsityTrade.Core.DTOs.Admin
{
    // This DTO defines the data admin provides when resolving a report
    public class ResolveReportDto
    {
        // The new status — Resolved or Dismissed
        public string Status { get; set; } = string.Empty;

        // Admin notes explaining the resolution decision
        public string? AdminNotes { get; set; }
    }
}