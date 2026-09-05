using VarsityTrade.Core.DTOs.Reviews; // Provides Review DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all review operations
    public interface IReviewService
    {
        // Get all reviews received by a seller — for the seller's public profile
        Task<IEnumerable<ReviewResponseDto>> GetReviewsBySellerAsync(int sellerProfileId);

        // Get all reviews written by a buyer — for the My Reviews page
        Task<IEnumerable<ReviewResponseDto>> GetReviewsByReviewerAsync(int reviewerId);

        // Get a single review by ID
        Task<ReviewResponseDto?> GetReviewByIdAsync(int reviewId);

        // Create a new review — buyer only, gated by completed transaction
        Task<ReviewResponseDto?> CreateReviewAsync(int reviewerId, ReviewRequestDto request);
    }
}