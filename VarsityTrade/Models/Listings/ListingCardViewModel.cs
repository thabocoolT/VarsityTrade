using Newtonsoft.Json;

namespace VarsityTrade.Web.Models.Listings
{
    public class ListingCardViewModel
    {
        [JsonProperty("listingId")]
        public int ListingId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("price")]
        public decimal Price { get; set; }

        [JsonProperty("listingType")]
        public string ListingType { get; set; } = string.Empty;

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

        [JsonProperty("sellerRating")]
        public decimal SellerRating { get; set; }

        [JsonProperty("coverImageUrl")]
        public string? CoverImageUrl { get; set; }

        [JsonProperty("viewCount")]
        public int ViewCount { get; set; }

        [JsonProperty("isFeatured")]
        public bool IsFeatured { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("sellerProfileId")]
        public int SellerProfileId { get; set; }

        [JsonProperty("isLoggedIn")]
        public bool IsLoggedIn { get; set; }
    }
}