using System.ComponentModel.DataAnnotations; // Provides Required and Range validation

namespace VarsityTrade.Core.DTOs.Reviews
{
    // This DTO defines the data a buyer must provide when leaving a review
    // A review can only be submitted after a completed transaction
    public class ReviewRequestDto
    {
        // TransactionId links the review to a specific completed transaction
        // This is the gate — no transaction means no review
        [Required(ErrorMessage = "Transaction ID is required")]
        public int TransactionId { get; set; }

        // Rating must be between 1 and 5 stars
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        // Comment is optional — buyers can leave a rating without written feedback
        [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
        public string? Comment { get; set; }
    }
}