namespace VarsityTrade.Web.Models.Listings
{
    // Represents the full listing detail page
    public class ListingDetailViewModel
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ListingType { get; set; } = string.Empty;
        public bool IsNegotiable { get; set; }
        public bool IsFeatured { get; set; }
        public int Quantity { get; set; }
        public int ViewCount { get; set; }
        public bool CampusPickup { get; set; }
        public bool DeliveryAvailable { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string UniversityName { get; set; } = string.Empty;
        public string UniversityShortName { get; set; } = string.Empty;
        public int SellerProfileId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal SellerRating { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}