using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Report
    {
        public int ReportId { get; set; }
        public int ReportedById { get; set; }
        public int? ListingId { get; set; }
        public int?ReportedUserId { get; set; }
        public string ReportType { get; set; } = null!;
        public string Description { get; set; }=null!;
        public string? EvidenceImage { get; set; }
        public string status { get; set; } = "Open";
        public string? AdminNotes { get; set; }
        public int? ResolvedById { get; set; }

        public DateTime CreatedAt { get; set; }= DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public User ReportedBy { get; set; } = null!;
        public Listing? Listing { get; set; }
        public User? ReportedUser { get; set; }
        public User? ResolvedBy { get; set; }


    }
}
