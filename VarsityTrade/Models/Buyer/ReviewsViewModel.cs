using Newtonsoft.Json;

namespace VarsityTrade.Web.Models.Reviews
{
    public class ReviewViewModel
    {
        [JsonProperty("reviewId")]
        public int ReviewId { get; set; }

        [JsonProperty("rating")]
        public int Rating { get; set; }

        [JsonProperty("comment")]
        public string? Comment { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("transactionId")]
        public int TransactionId { get; set; }

        [JsonProperty("reviewerId")]
        public int ReviewerId { get; set; }

        [JsonProperty("reviewerFirstName")]
        public string ReviewerFirstName { get; set; } = string.Empty;

        [JsonProperty("reviewerLastName")]
        public string ReviewerLastName { get; set; } = string.Empty;

        [JsonProperty("reviewerSuburb")]
        public string ReviewerSuburb { get; set; } = string.Empty;

        [JsonProperty("reviewerResidenceName")]
        public string? ReviewerResidenceName { get; set; }

        [JsonProperty("sellerProfileId")]
        public int SellerProfileId { get; set; }

        [JsonProperty("storeName")]
        public string StoreName { get; set; } = string.Empty;

        [JsonProperty("listingTitle")]
        public string ListingTitle { get; set; } = string.Empty;

        // Filled by the Web controller from the transaction endpoint.
        public decimal FinalPrice { get; set; }
    }

    public class TransactionViewModel
    {
        [JsonProperty("transactionId")]
        public int TransactionId { get; set; }

        [JsonProperty("finalPrice")]
        public decimal FinalPrice { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("completedAt")]
        public DateTime CompletedAt { get; set; }

        [JsonProperty("listingId")]
        public int ListingId { get; set; }

        [JsonProperty("listingTitle")]
        public string ListingTitle { get; set; } = string.Empty;

        [JsonProperty("buyerId")]
        public int BuyerId { get; set; }

        [JsonProperty("buyerFirstName")]
        public string BuyerFirstName { get; set; } = string.Empty;

        [JsonProperty("buyerLastName")]
        public string BuyerLastName { get; set; } = string.Empty;

        [JsonProperty("sellerProfileId")]
        public int SellerProfileId { get; set; }

        [JsonProperty("storeName")]
        public string StoreName { get; set; } = string.Empty;

        [JsonProperty("hasReview")]
        public bool HasReview { get; set; }
    }
}