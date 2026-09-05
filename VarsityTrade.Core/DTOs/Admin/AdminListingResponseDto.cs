namespace VarsityTrade.Core.DTOs.Admin
{
    // This DTO defines what the API returns for each listing in the admin panel
    // Used on the Manage Listings page
    public class AdminListingResponseDto
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // University info — for cross-campus admin view
        public string UniversityName { get; set; } = string.Empty;
        public string UniversityShortName { get; set; } = string.Empty;

        // Seller info — shown in the listings table
        public int SellerProfileId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string SellerFirstName { get; set; } = string.Empty;
        public string SellerLastName { get; set; } = string.Empty;
    }
}