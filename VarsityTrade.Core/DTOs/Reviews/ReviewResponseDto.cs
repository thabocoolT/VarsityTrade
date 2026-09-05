namespace VarsityTrade.Core.DTOs.Reviews
{
    // This DTO defines what the API returns when a review is requested
    // Used on the seller's public profile and on the My Reviews pages
    public class ReviewResponseDto
    {
        // Core review identity
        public int ReviewId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }

        // Transaction info — confirms the review is linked to a real trade
        public int TransactionId { get; set; }

        // Reviewer info — shown on the seller's profile and review cards
        public int ReviewerId { get; set; }
        public string ReviewerFirstName { get; set; } = string.Empty;
        public string ReviewerLastName { get; set; } = string.Empty;
        public string ReviewerSuburb { get; set; } = string.Empty;
        public string? ReviewerResidenceName { get; set; }

        // Seller info — shown when the buyer views their own submitted reviews
        public int SellerProfileId { get; set; }
        public string StoreName { get; set; } = string.Empty;

        // Listing info — shows what item the review is about
        public string ListingTitle { get; set; } = string.Empty;
    }
}