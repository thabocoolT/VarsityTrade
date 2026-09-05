namespace VarsityTrade.Core.DTOs.Admin
{
    // This DTO defines what the API returns for each report in the admin panel
    // Used on the Reports Queue page
    public class ReportResponseDto
    {
        public int ReportId { get; set; }
        public string ReportType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? EvidenceImage { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AdminNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // Who submitted the report
        public int ReportedById { get; set; }
        public string ReportedByFirstName { get; set; } = string.Empty;
        public string ReportedByLastName { get; set; } = string.Empty;

        // The listing being reported — nullable
        public int? ListingId { get; set; }
        public string? ListingTitle { get; set; }

        // The user being reported — nullable
        public int? ReportedUserId { get; set; }
        public string? ReportedUserFirstName { get; set; }
        public string? ReportedUserLastName { get; set; }

        // Who resolved the report — nullable until resolved
        public int? ResolvedById { get; set; }
        public string? ResolvedByFirstName { get; set; }
    }
}