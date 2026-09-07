using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using System.Security.Claims; // Provides ClaimTypes for reading JWT claims
using VarsityTrade.Core.DTOs.Offers; // Provides Offer DTOs
using VarsityTrade.Core.Interfaces; // Provides IOfferService

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // All routes start with /api/offers
    [Authorize] // All offer endpoints require authentication
    public class OffersController : ControllerBase
    {
        // IOfferService injected — controller stays thin
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        // Helper to read user ID from JWT token
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/offers/my
        // Returns all offers made by the logged in buyer
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns all offers made by the logged in buyer.</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyOffers()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var offers = await _offerService.GetOffersByBuyerAsync(userId.Value);
            return Ok(offers);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/offers/received
        // Returns all offers received by the logged in seller
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns all offers received by the logged in seller.</summary>
        [HttpGet("received")]
        public async Task<IActionResult> GetReceivedOffers()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var offers = await _offerService.GetOffersBySellerAsync(userId.Value);
            return Ok(offers);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/offers/{id}
        // Returns a single offer by ID — user must be buyer or seller
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns a single offer by ID — user must be buyer or seller.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOfferById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var offer = await _offerService.GetOfferByIdAsync(id, userId.Value);

            if (offer == null)
                return NotFound(new { message = "Offer not found." });

            return Ok(offer);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/offers
        // Creates a new offer on a listing
        // ─────────────────────────────────────────────────────────────
        /// <summary>Creates a new cash, trade, or combined offer on a listing.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateOffer([FromBody] OfferRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var result = await _offerService.CreateOfferAsync(userId.Value, request);

            if (result == null)
                return BadRequest(new { message = "Could not create offer. Listing may not exist or you may be the seller." });

            return CreatedAtAction(nameof(GetOfferById), new { id = result.OfferId }, result);
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/offers/{id}/accept
        // Seller accepts an offer — creates a Transaction automatically
        // ─────────────────────────────────────────────────────────────
        /// <summary>Seller accepts an offer — creates a transaction and rejects all competing offers.</summary>
        [HttpPut("{id}/accept")]
        public async Task<IActionResult> AcceptOffer(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var success = await _offerService.AcceptOfferAsync(id, userId.Value);

            if (!success)
                return NotFound(new { message = "Offer not found or cannot be accepted." });

            return Ok(new { message = "Offer accepted. Transaction created successfully." });
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/offers/{id}/reject
        // Seller rejects an offer
        // ─────────────────────────────────────────────────────────────
        /// <summary>Seller rejects a pending offer.</summary>
        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectOffer(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var success = await _offerService.RejectOfferAsync(id, userId.Value);

            if (!success)
                return NotFound(new { message = "Offer not found or cannot be rejected." });

            return Ok(new { message = "Offer rejected." });
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/offers/{id}/cancel
        // Buyer cancels their own pending offer
        // ─────────────────────────────────────────────────────────────
        /// <summary>Buyer cancels their own pending offer.</summary>
        [HttpPut("{id}/cancel")]
        public async Task<IActionResult> CancelOffer(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var success = await _offerService.CancelOfferAsync(id, userId.Value);

            if (!success)
                return NotFound(new { message = "Offer not found or cannot be cancelled." });

            return Ok(new { message = "Offer cancelled." });
        }
    }
}