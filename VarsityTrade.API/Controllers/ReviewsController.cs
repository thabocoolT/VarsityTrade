using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using System.Security.Claims; // Provides ClaimTypes for reading JWT claims
using VarsityTrade.Core.DTOs.Reviews; // Provides Review DTOs
using VarsityTrade.Core.Interfaces; // Provides IReviewService

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // All routes start with /api/reviews
    [Authorize] // All review endpoints require authentication
    public class ReviewsController : ControllerBase
    {
        // IReviewService injected — controller stays thin
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // Helper to read user ID from JWT token
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/reviews/seller/{sellerProfileId}
        // Returns all reviews for a specific seller — public profile page
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns all reviews received by a specific seller — for the public profile page.</summary>
        [HttpGet("seller/{sellerProfileId}")]
        public async Task<IActionResult> GetReviewsBySeller(int sellerProfileId)
        {
            var reviews = await _reviewService.GetReviewsBySellerAsync(sellerProfileId);
            return Ok(reviews);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/reviews/my
        // Returns all reviews written by the logged in buyer
        // Used on the My Reviews page in the buyer dashboard
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns all reviews written by the logged in buyer.</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var reviews = await _reviewService.GetReviewsByReviewerAsync(userId.Value);
            return Ok(reviews);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/reviews/{id}
        // Returns a single review by ID
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns a single review by ID.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);

            if (review == null)
                return NotFound(new { message = "Review not found." });

            return Ok(review);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/reviews
        // Creates a new review — buyer only, gated by completed transaction
        // ─────────────────────────────────────────────────────────────
        /// <summary>Creates a new review — gated by completed transaction. One review per transaction.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var result = await _reviewService.CreateReviewAsync(userId.Value, request);

            if (result == null)
                return BadRequest(new { message = "Could not create review. Transaction may not exist, may not be yours, or a review has already been submitted." });

            return CreatedAtAction(nameof(GetReviewById), new { id = result.ReviewId }, result);
        }
    }
}