namespace VarsityTrade.Web.Models.Listings
{
    // Represents a single listing card shown on browse, search, and category pages
    public class ListingCardViewModel
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Condition { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string UniversityShortName { get; set; } = string.Empty;
        public string StoreName { get; set; } = string.Empty;
        public decimal SellerRating { get; set; }
        public string? CoverImageUrl { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}