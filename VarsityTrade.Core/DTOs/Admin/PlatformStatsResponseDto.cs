namespace VarsityTrade.Core.DTOs.Admin
{
    public class PlatformStatsResponseDto
    {
        // User stats
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int VerificationRate { get; set; }
        public int BannedUsers { get; set; }
        public int DeactivatedUsers { get; set; }
        public int NewUsersThisWeek { get; set; }

        // Listing stats
        public int TotalListings { get; set; }
        public int ActiveListings { get; set; }
        public int SoldListings { get; set; }
        public int DeletedListings { get; set; }

        // Transaction stats
        public int TotalTransactions { get; set; }
        public int CompletedTransactions { get; set; }

        // Message stats
        public int TotalMessages { get; set; }
        public int TotalConversations { get; set; }

        // Report stats
        public int OpenReports { get; set; }
        public int ResolvedReports { get; set; }
        public int TotalReports { get; set; }

        // Seller stats
        public int TotalSellerProfiles { get; set; }
        public int ActiveSellerProfiles { get; set; }
    }
}