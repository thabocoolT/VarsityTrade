using Microsoft.EntityFrameworkCore; // Provides Include, FirstOrDefaultAsync, ToListAsync
using VarsityTrade.Core.DTOs.Reviews; // Provides Review DTOs
using VarsityTrade.Core.Entities; // Provides Review entity
using VarsityTrade.Core.Interfaces; // Provides IReviewService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.Application.Services
{
    // ReviewService handles all business logic for review operations
    // Reviews are gated by completed transactions — no transaction means no review
    public class ReviewService : IReviewService
    {
        // DbContext injected for all database operations
        private readonly VarsityTradeDbContext _context;

        // Constructor receives DbContext via dependency injection
        public ReviewService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET REVIEWS BY SELLER
        // Returns all reviews received by a specific seller
        // Used on the seller's public profile page
        // ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsBySellerAsync(int sellerProfileId)
        {
            // Load all reviews for this seller ordered by most recent first
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)               // Load reviewer for name display
                    .ThenInclude(u => u.Location)       // Load reviewer location for suburb and res
                .Include(r => r.SellerProfile)          // Load seller profile for store name
                .Include(r => r.Transaction)            // Load transaction for listing title
                    .ThenInclude(t => t.Listing)        // Load listing from transaction
                .Where(r =>
                    r.SellerProfileId == sellerProfileId
                    && r.DeletedAt == null)             // Exclude soft-deleted reviews
                .OrderByDescending(r => r.CreatedAt)    // Most recent reviews first
                .ToListAsync();

            return reviews.Select(r => MapToResponseDto(r));
        }

        // ─────────────────────────────────────────────────────────────
        // GET REVIEWS BY REVIEWER
        // Returns all reviews written by a specific buyer
        // Used on the buyer's My Reviews page
        // ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<ReviewResponseDto>> GetReviewsByReviewerAsync(int reviewerId)
        {
            // Load all reviews written by this buyer
            var reviews = await _context.Reviews
                .Include(r => r.Reviewer)
                    .ThenInclude(u => u.Location)
                .Include(r => r.SellerProfile)
                .Include(r => r.Transaction)
                    .ThenInclude(t => t.Listing)
                .Where(r =>
                    r.ReviewerId == reviewerId
                    && r.DeletedAt == null)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(r => MapToResponseDto(r));
        }

        // ─────────────────────────────────────────────────────────────
        // GET REVIEW BY ID
        // Returns a single review by its ID
        // ─────────────────────────────────────────────────────────────
        public async Task<ReviewResponseDto?> GetReviewByIdAsync(int reviewId)
        {
            var review = await _context.Reviews
                .Include(r => r.Reviewer)
                    .ThenInclude(u => u.Location)
                .Include(r => r.SellerProfile)
                .Include(r => r.Transaction)
                    .ThenInclude(t => t.Listing)
                .FirstOrDefaultAsync(r =>
                    r.ReviewId == reviewId
                    && r.DeletedAt == null);

            if (review == null)
                return null;

            return MapToResponseDto(review);
        }

        // ─────────────────────────────────────────────────────────────
        // CREATE REVIEW
        // Creates a new review — gated by completed transaction
        // A buyer can only review once per transaction
        // ─────────────────────────────────────────────────────────────
        public async Task<ReviewResponseDto?> CreateReviewAsync(int reviewerId, ReviewRequestDto request)
        {
            // Find the transaction — must belong to this buyer
            var transaction = await _context.Transactions
                .Include(t => t.Listing) // Load listing to get the seller profile
                .FirstOrDefaultAsync(t =>
                    t.TransactionId == request.TransactionId
                    && t.BuyerId == reviewerId          // Only the buyer can review
                    && t.Status == "Completed");        // Transaction must be completed

            // Return null if transaction not found or buyer is not the purchaser
            if (transaction == null)
                return null;

            // Check if a review already exists for this transaction
            // The unique constraint UQ_Review_Transaction enforces one review per transaction
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.TransactionId == request.TransactionId);

            // Return null if review already submitted — cannot review twice
            if (existingReview != null)
                return null;

            // Create the new review entity
            var review = new Review
            {
                TransactionId = request.TransactionId,
                ReviewerId = reviewerId,
                SellerProfileId = transaction.SellerProfileId, // Get seller from transaction
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Reviews.AddAsync(review);

            // Update the seller's cached AverageRating
            // Recalculate from all reviews for this seller for accuracy
            await UpdateSellerAverageRatingAsync(transaction.SellerProfileId);

            await _context.SaveChangesAsync();

            // Reload with all related data for the response
            return await GetReviewByIdAsync(review.ReviewId);
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — UPDATE SELLER AVERAGE RATING
        // Recalculates and updates the cached AverageRating on SellerProfile
        // Called after every new review to keep the cached value accurate
        // ─────────────────────────────────────────────────────────────
        private async Task UpdateSellerAverageRatingAsync(int sellerProfileId)
        {
            // Get all non-deleted reviews for this seller
            var allReviews = await _context.Reviews
                .Where(r => r.SellerProfileId == sellerProfileId && r.DeletedAt == null)
                .ToListAsync();

            // Find the seller profile to update
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.SellerProfileId == sellerProfileId);

            if (sellerProfile == null)
                return;

            // Recalculate average rating from all reviews
            // If no reviews yet default to 0.00
            sellerProfile.AverageRating = allReviews.Any()
                ? Math.Round((decimal)allReviews.Average(r => r.Rating), 2)
                : 0.00m;

            sellerProfile.UpdatedAt = DateTime.UtcNow;
            _context.SellerProfiles.Update(sellerProfile);
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — MAP TO RESPONSE DTO
        // ─────────────────────────────────────────────────────────────
        private static ReviewResponseDto MapToResponseDto(Review review)
        {
            return new ReviewResponseDto
            {
                ReviewId = review.ReviewId,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt,
                TransactionId = review.TransactionId,

                // Reviewer info — includes location for suburb and res display
                ReviewerId = review.ReviewerId,
                ReviewerFirstName = review.Reviewer?.FirstName ?? string.Empty,
                ReviewerLastName = review.Reviewer?.LastName ?? string.Empty,
                ReviewerSuburb = review.Reviewer?.Location?.Suburb ?? string.Empty,
                ReviewerResidenceName = review.Reviewer?.Location?.ResidenceName,

                // Seller info
                SellerProfileId = review.SellerProfileId,
                StoreName = review.SellerProfile?.StoreName ?? string.Empty,

                // Listing info from the transaction
                ListingTitle = review.Transaction?.Listing?.Title ?? string.Empty,
            };
        }
    }
}