using Newtonsoft.Json; // Provides JsonProperty for API field name mapping

namespace VarsityTrade.Web.Models.Listings
{
    // Represents a single listing card shown on browse, search, and category pages
    // JsonProperty attributes map the API's camelCase response to PascalCase C# properties
    public class ListingCardViewModel
    {
        [JsonProperty("listingId")]
        public int ListingId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; } = string.Empty;

        [JsonProperty("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("universityShortName")]
        public string UniversityShortName { get; set; } = string.Empty;

        [JsonProperty("storeName")]
        public string StoreName { get; set; } = string.Empty;

        [JsonProperty("averageRating")]
        public decimal SellerRating { get; set; }

        [JsonProperty("coverImageUrl")]
        public string? CoverImageUrl { get; set; }

        [JsonProperty("viewCount")]
        public int ViewCount { get; set; }

        [JsonProperty("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }
    }
}