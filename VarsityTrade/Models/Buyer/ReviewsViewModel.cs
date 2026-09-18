using Newtonsoft.Json;

namespace VarsityTrade.Web.Models.Reviews
{
    public class ReviewViewModel
    {
        [JsonProperty("reviewId")] public int ReviewId { get; set; }
        [JsonProperty("rating")] public int Rating { get; set; }
        [JsonProperty("comment")] public string? Comment { get; set; }
        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }
        [JsonProperty("listingTitle")] public string ListingTitle { get; set; } = string.Empty;
        [JsonProperty("storeName")] public string StoreName { get; set; } = string.Empty;
        [JsonProperty("sellerProfileId")] public int SellerProfileId { get; set; }
    }
}