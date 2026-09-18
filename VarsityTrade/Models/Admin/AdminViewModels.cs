using Newtonsoft.Json; // Provides JsonProperty for API field mapping

namespace VarsityTrade.Web.Models.Admin
{
    // Platform statistics view model
    public class PlatformStatsViewModel
    {
        [JsonProperty("totalUsers")] public int TotalUsers { get; set; }
        [JsonProperty("activeUsers")] public int ActiveUsers { get; set; }
        [JsonProperty("bannedUsers")] public int BannedUsers { get; set; }
        [JsonProperty("deactivatedUsers")] public int DeactivatedUsers { get; set; }
        [JsonProperty("newUsersThisWeek")] public int NewUsersThisWeek { get; set; }
        [JsonProperty("totalListings")] public int TotalListings { get; set; }
        [JsonProperty("activeListings")] public int ActiveListings { get; set; }
        [JsonProperty("soldListings")] public int SoldListings { get; set; }
        [JsonProperty("deletedListings")] public int DeletedListings { get; set; }
        [JsonProperty("totalTransactions")] public int TotalTransactions { get; set; }
        [JsonProperty("totalMessages")] public int TotalMessages { get; set; }
        [JsonProperty("totalConversations")] public int TotalConversations { get; set; }
        [JsonProperty("openReports")] public int OpenReports { get; set; }
        [JsonProperty("resolvedReports")] public int ResolvedReports { get; set; }
        [JsonProperty("totalReports")] public int TotalReports { get; set; }
        [JsonProperty("totalSellerProfiles")] public int TotalSellerProfiles { get; set; }
        [JsonProperty("activeSellerProfiles")] public int ActiveSellerProfiles { get; set; }
        [JsonProperty("completedTransactions")] public decimal CompletedTransactions { get; set; }
        [JsonProperty("verifiedUsers")] public int VerifiedUsers { get; set; }
    }

    // Admin user view model
    public class AdminUserViewModel
    {
        [JsonProperty("userId")] public int UserId { get; set; }
        [JsonProperty("firstName")] public string FirstName { get; set; } = string.Empty;
        [JsonProperty("lastName")] public string LastName { get; set; } = string.Empty;
        [JsonProperty("email")] public string Email { get; set; } = string.Empty;
        [JsonProperty("role")] public string Role { get; set; } = string.Empty;
        [JsonProperty("isActive")] public bool IsActive { get; set; }
        [JsonProperty("isBanned")] public bool IsBanned { get; set; }
        [JsonProperty("studentVerified")] public bool StudentVerified { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("universityName")] public string UniversityName { get; set; } = string.Empty;
        [JsonProperty("universityShortName")] public string UniversityShortName { get; set; } = string.Empty;
        [JsonProperty("suburb")] public string? Suburb { get; set; }
        [JsonProperty("residenceName")] public string? ResidenceName { get; set; }
        [JsonProperty("hasSellerProfile")] public bool HasSellerProfile { get; set; }
        [JsonProperty("storeName")] public string? StoreName { get; set; }
    }

    // Admin listing view model
    public class AdminListingViewModel
    {
        [JsonProperty("listingId")] public int ListingId { get; set; }
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("price")] public decimal Price { get; set; }
        [JsonProperty("status")] public string Status { get; set; } = string.Empty;
        [JsonProperty("condition")] public string Condition { get; set; } = string.Empty;
        [JsonProperty("categoryName")] public string CategoryName { get; set; } = string.Empty;
        [JsonProperty("viewCount")] public int ViewCount { get; set; }
        [JsonProperty("isFeatured")] public bool IsFeatured { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("deletedAt")] public DateTime? DeletedAt { get; set; }
        [JsonProperty("universityName")] public string UniversityName { get; set; } = string.Empty;
        [JsonProperty("universityShortName")] public string UniversityShortName { get; set; } = string.Empty;
        [JsonProperty("sellerProfileId")] public int SellerProfileId { get; set; }
        [JsonProperty("storeName")] public string StoreName { get; set; } = string.Empty;
        [JsonProperty("sellerFirstName")] public string SellerFirstName { get; set; } = string.Empty;
        [JsonProperty("sellerLastName")] public string SellerLastName { get; set; } = string.Empty;
    }

    // Admin report view model
    public class AdminReportViewModel
    {
        [JsonProperty("reportId")] public int ReportId { get; set; }
        [JsonProperty("reportType")] public string ReportType { get; set; } = string.Empty;
        [JsonProperty("description")] public string Description { get; set; } = string.Empty;
        [JsonProperty("status")] public string Status { get; set; } = string.Empty;
        [JsonProperty("adminNotes")] public string? AdminNotes { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("resolvedAt")] public DateTime? ResolvedAt { get; set; }
        [JsonProperty("reportedByFirstName")] public string ReportedByFirstName { get; set; } = string.Empty;
        [JsonProperty("reportedByLastName")] public string ReportedByLastName { get; set; } = string.Empty;
        [JsonProperty("listingId")] public int? ListingId { get; set; }
        [JsonProperty("listingTitle")] public string? ListingTitle { get; set; }
        [JsonProperty("reportedUserFirstName")] public string? ReportedUserFirstName { get; set; }
        [JsonProperty("reportedUserLastName")] public string? ReportedUserLastName { get; set; }
    }

    // Hero banner slide view model
    public class HeroBannerSlideViewModel
    {
        [JsonProperty("heroBannerSlideId")] public int HeroBannerSlideId { get; set; }
        [JsonProperty("slideType")] public string SlideType { get; set; } = string.Empty;
        [JsonProperty("title")] public string Title { get; set; } = string.Empty;
        [JsonProperty("subtitle")] public string? Subtitle { get; set; }
        [JsonProperty("imageUrl")] public string? ImageUrl { get; set; }
        [JsonProperty("buttonText")] public string? ButtonText { get; set; }
        [JsonProperty("buttonUrl")] public string? ButtonUrl { get; set; }
        [JsonProperty("isVisible")] public bool IsVisible { get; set; }
        [JsonProperty("sortOrder")] public int SortOrder { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("listingTitle")] public string? ListingTitle { get; set; }
    }

    public class AdminAuditLogViewModel
    {
        public int AuditLogId { get; set; }
        public int? UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public DateTime Created { get; set; }

        public string? UserName { get; set; }
    }

    // Admin dashboard view model
    public class AdminDashboardViewModel
    {
        public PlatformStatsViewModel Stats { get; set; } = new();
        public List<AdminReportViewModel> OpenReports { get; set; } = new();
        public List<AdminUserViewModel> RecentUsers { get; set; } = new();
        public List<AdminAuditLogViewModel> RecentActivity { get; set; }
                = new List<AdminAuditLogViewModel>();
    }

    public class SystemSettingsViewModel
    {
        [JsonProperty("key")] public string Key { get; set; } = string.Empty;
        [JsonProperty("value")] public string Value { get; set; } = string.Empty;
        [JsonProperty("description")] public string? Description { get; set; }
        [JsonProperty("updatedAt")] public DateTime? UpdatedAt { get; set; }
    }
}