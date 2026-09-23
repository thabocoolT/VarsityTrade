using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VarsityTrade.Core.DTOs.Listings;
using VarsityTrade.Core.Interfaces;
using VarsityTrade.Infrastructure.Data;


namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ListingsController : ControllerBase
    {
        private readonly IListingService _listingService;
        private readonly VarsityTradeDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ListingsController(IListingService listingService, VarsityTradeDbContext context, IWebHostEnvironment environment)
        {
            _listingService = listingService;
            _context = context;
            _environment = environment;
        }

        

        private int? GetCurrentUserId()
        {
            var claim =
                User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

            return claim != null &&
                   int.TryParse(claim.Value, out var userId)
                ? userId
                : null;
        }

        private int? GetCurrentUniversityId()
        {
            var claim =
                User.FindFirst("universityId");

            return claim != null &&
                   int.TryParse(claim.Value, out var universityId)
                ? universityId
                : null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/listings/public
        // Guests can browse all active listings.
        // ─────────────────────────────────────────────────────────────
        [AllowAnonymous]
        [HttpGet("public")]
        public async Task<IActionResult> GetPublicListings()
        {
            var listings =
                await _listingService
                    .GetAllActiveListingsAsync();

            return Ok(listings);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/listings/university/{universityId}
        // Authenticated users can only access their own university.
        // ─────────────────────────────────────────────────────────────
        [Authorize]
        [HttpGet("university/{universityId}")]
        
        public async Task<IActionResult> GetListingsByUniversity(
            int universityId)
        {
            var userUniversityId =
                GetCurrentUniversityId();

            if (userUniversityId == null)
                return Unauthorized();

            if (userUniversityId.Value != universityId)
                return Forbid();

            var listings =
                await _listingService
                    .GetListingsByUniversityAsync(
                        universityId);

            return Ok(listings);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/listings/{id}
        // Guests can view active listings.
        // Authenticated users can view active listings in their
        // university. Sellers can also view their own listings.
        // ─────────────────────────────────────────────────────────────
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetListingById(int id)
        {
            var listing =
                await _listingService
                    .GetListingByIdAsync(id);

            if (listing == null)
                return NotFound(
                    new { message = "Listing not found." });

            var currentUserId =
                GetCurrentUserId();

            var isAuthenticated =
                User.Identity?.IsAuthenticated == true;

            var isOwner = false;

            if (currentUserId.HasValue)
            {
                isOwner =
                    await _context.SellerProfiles.AnyAsync(
                        sp =>
                            sp.UserId == currentUserId.Value
                            && sp.SellerProfileId ==
                               listing.SellerProfileId
                            && sp.IsActive);
            }

            if (!isAuthenticated)
            {
                if (listing.Status != "Active")
                    return NotFound(
                        new { message = "Listing not found." });
            }
            else if (!isOwner)
            {
                var userUniversityId =
                    GetCurrentUniversityId();

                if (userUniversityId == null ||
                    listing.UniversityId !=
                    userUniversityId.Value)
                {
                    return NotFound(
                        new { message = "Listing not found." });
                }

                if (listing.Status != "Active")
                    return NotFound(
                        new { message = "Listing not found." });
            }

            // Only increment views after visibility has been confirmed.
            await _listingService
                .IncrementViewCountAsync(id);

            // Return the latest view count.
            listing =
                await _listingService
                    .GetListingByIdAsync(id);

            return Ok(listing);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/listings
        // ─────────────────────────────────────────────────────────────
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateListing(
            [FromBody] ListingRequestDto request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized(
                    new
                    {
                        message =
                            "User identity not found in token."
                    });

            var result =
                await _listingService
                    .CreateListingAsync(
                        userId.Value,
                        request);

            if (result == null)
                return BadRequest(
                    new
                    {
                        message =
                            "Could not create listing. " +
                            "Ensure your seller profile is active."
                    });

            return CreatedAtAction(
                nameof(GetListingById),
                new { id = result.ListingId },
                result);
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/listings/{id}
        // ─────────────────────────────────────────────────────────────
        [Authorize]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateListing(
            int id,
            [FromBody] ListingRequestDto request)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized(
                    new
                    {
                        message =
                            "User identity not found in token."
                    });

            // IMPORTANT:
            // Service signature is:
            // UpdateListingAsync(listingId, userId, request)
            var result =
                await _listingService
                    .UpdateListingAsync(
                        id,
                        userId.Value,
                        request);

            if (result == null)
                return NotFound(
                    new
                    {
                        message =
                            "Listing not found or you are not " +
                            "authorized to update it."
                    });

            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE /api/listings/{id}
        // ─────────────────────────────────────────────────────────────
        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteListing(int id)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized(
                    new
                    {
                        message =
                            "User identity not found in token."
                    });

            // IMPORTANT:
            // Service signature is:
            // DeleteListingAsync(listingId, userId)
            var success =
                await _listingService
                    .DeleteListingAsync(
                        id,
                        userId.Value);

            if (!success)
                return NotFound(
                    new
                    {
                        message =
                            "Listing not found or you are not " +
                            "authorized to delete it."
                    });

            return NoContent();
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/listings/my
        // ─────────────────────────────────────────────────────────────
        [Authorize]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyListings()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var sellerProfile =
                await _context.SellerProfiles
                    .FirstOrDefaultAsync(
                        sp =>
                            sp.UserId == userId.Value
                            && sp.IsActive);

            if (sellerProfile == null)
                return Ok(new List<object>());

            var listings =
                await _context.Listings
                    .Include(l => l.ListingStatus)
                    .Include(l => l.Category)
                    .Include(l => l.Condition)
                    .Include(l => l.ListingImages)
                    .Where(l =>
                        l.SellerProfileId ==
                        sellerProfile.SellerProfileId)
                    .OrderByDescending(l => l.CreatedAt)
                    .Select(l => new
                    {
                        l.ListingId,
                        l.Title,
                        l.Price,
                        l.ViewCount,
                        l.IsFeatured,
                        l.CreatedAt,
                        l.DeletedAt,

                        Status =
                            l.ListingStatus != null
                                ? l.ListingStatus.Name
                                : "Unknown",

                        Condition =
                            l.Condition != null
                                ? l.Condition.Name
                                : "Unknown",

                        CategoryName =
                            l.Category != null
                                ? l.Category.Name
                                : "Unknown",

                        CoverImageUrl =
                            l.ListingImages != null
                                ? l.ListingImages
                                    .Where(i => i.IsCoverImage)
                                    .Select(i => i.ImagePath)
                                    .FirstOrDefault()
                                  ??
                                  l.ListingImages
                                    .Select(i => i.ImagePath)
                                    .FirstOrDefault()
                                : null
                    })
                    .ToListAsync();

            return Ok(listings);
        }
        /// <summary>Uploads an image for a listing and returns the saved URL.</summary>
        [HttpPost("upload-image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded." });

            // Allowed extensions
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
                return BadRequest(new { message = "Invalid file type. Only JPG, PNG and WEBP are allowed." });

            // Save to wwwroot/uploads/listings/
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "listings");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var url = $"/uploads/listings/{fileName}";
            return Ok(new { url });
        }


    }
}