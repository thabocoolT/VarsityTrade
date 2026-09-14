using Microsoft.EntityFrameworkCore; // Provides Include, FirstOrDefaultAsync, ToListAsync
using VarsityTrade.Core.DTOs.Listings; // Provides Listing DTOs
using VarsityTrade.Core.Entities; // Provides Listing entity
using VarsityTrade.Core.Interfaces; // Provides IListingService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.Application.Services
{
    // ListingService handles all business logic for listing operations
    // It reads and writes to the database via the DbContext
    public class ListingService : IListingService
    {
        // DbContext is injected to allow database operations
        private readonly VarsityTradeDbContext _context;

        // Constructor receives the DbContext via dependency injection
        public ListingService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET LISTINGS BY UNIVERSITY
        // Returns all active listings for a given university
        // This is the main campus-locked marketplace feed
        // ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<ListingResponseDto>> GetListingsByUniversityAsync(int universityId)
        {
            // Query all active listings for this university
            // Include related entities to avoid extra queries (eager loading)
            // Filter out soft-deleted listings using DeletedAt
            return await _context.Listings
                .Include(l => l.SellerProfile)      // Load seller info for the listing card
                    .ThenInclude(sp => sp.User)      // Load seller's user for their name
                .Include(l => l.Category)            // Load category name for display
                .Include(l => l.Condition)           // Load condition name for display
                .Include(l => l.ListingStatus)       // Load status to filter active only
                .Include(l => l.University)          // Load university for campus lock display
                .Include(l => l.ListingImages)              // Load images to get the cover image
                .Where(l =>
                    l.UniversityId == universityId   // Campus lock — only show this university's listings
                    && l.DeletedAt == null           // Exclude soft-deleted listings
                    && l.ListingStatus.Name == "Active") // Only show active listings
                .OrderByDescending(l => l.CreatedAt) // Newest listings first
                .Select(l => MapToResponseDto(l))    // Map to DTO — never return entities directly
                .ToListAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // GET LISTING BY ID
        // Returns a single listing by its ID for the detail page
        // ─────────────────────────────────────────────────────────────
        public async Task<ListingResponseDto?> GetListingByIdAsync(int listingId)
        {
            // Find the listing by ID with all related data loaded
            var listing = await _context.Listings
                .Include(l => l.SellerProfile)
                    .ThenInclude(sp => sp.User)
                .Include(l => l.Category)
                .Include(l => l.Condition)
                .Include(l => l.ListingStatus)
                .Include(l => l.University)
                .Include(l => l.ListingImages)
                .FirstOrDefaultAsync(l =>
                    l.ListingId == listingId
                    && l.DeletedAt == null); // Exclude soft-deleted listings

            // Return null if not found — controller will return 404
            if (listing == null)
                return null;

            return MapToResponseDto(listing);
        }

        // ─────────────────────────────────────────────────────────────
        // CREATE LISTING
        // Creates a new listing for a seller
        // ─────────────────────────────────────────────────────────────
        public async Task<ListingResponseDto?> CreateListingAsync(int userId, ListingRequestDto request)
        {
            // Find the seller profile to get the university ID
            // All listings inherit the seller's university for campus locking
            var sellerProfile = await _context.SellerProfiles
                .Include(sp => sp.User) // Load the user to get their university)
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            // Return null if seller profile not found or inactive
            if (sellerProfile == null)
                return null;

            // Get the Active status ID — new listings start as Active
            var activeStatus = await _context.ListingStatuses
                .FirstOrDefaultAsync(ls => ls.Name == "Active");

            if (activeStatus == null)
                return null;

            // Create the new listing entity from the request DTO
            var listing = new Listing
            {
                SellerProfileId = sellerProfile.SellerProfileId,
                UniversityId = sellerProfile.User.UniversityId, // Must come from the seller's user
                CategoryId = request.CategoryId,
                ConditionId = request.ConditionId,
                ListingStatusId = activeStatus.ListingStatusId,
                Title = request.Title,
                Description = request.Description,
                Price = request.Price,
                ListingType = request.ListingType,
                IsNegotiable = request.IsNegotiable,
                Quantity = request.Quantity,
                CampusPickup = request.CampusPickup,
                DeliveryAvailable = request.DeliveryAvailable,
                ViewCount = 0,
                IsFeatured = false,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(90),
            };

            // Save the listing to the database
            await _context.Listings.AddAsync(listing);
            await _context.SaveChangesAsync();

            // Reload the listing with all related entities for the response
            return await GetListingByIdAsync(listing.ListingId);
        }

        // ─────────────────────────────────────────────────────────────
        // UPDATE LISTING
        // Updates an existing listing — only the owner can update it
        // ─────────────────────────────────────────────────────────────
        public async Task<ListingResponseDto?> UpdateListingAsync(int listingId, int userId, ListingRequestDto request)
        {
            //Find the seller profile to ensure the user is authorized to update this listing
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            if (sellerProfile == null)
                return null; // User is not a seller or seller profile is inactive

            // Find the listing — must belong to this seller
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l =>
                    l.ListingId == listingId
                    && l.SellerProfileId == sellerProfile.SellerProfileId
                    && l.DeletedAt == null);

            // Return null if listing not found or does not belong to this seller
            if (listing == null)
                return null;

            // Update only the fields the seller is allowed to change
            listing.Title = request.Title;
            listing.Description = request.Description;
            listing.Price = request.Price;
            listing.CategoryId = request.CategoryId;
            listing.ConditionId = request.ConditionId;
            listing.ListingType = request.ListingType;
            listing.IsNegotiable = request.IsNegotiable;
            listing.Quantity = request.Quantity;
            listing.CampusPickup = request.CampusPickup;
            listing.DeliveryAvailable = request.DeliveryAvailable;
            listing.UpdatedAt = DateTime.UtcNow; // Track when the listing was last updated

            // Save the changes to the database
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();

            // Return the updated listing
            return await GetListingByIdAsync(listingId);
        }

        // ─────────────────────────────────────────────────────────────
        // DELETE LISTING
        // Soft deletes a listing — sets DeletedAt instead of removing the row
        // This preserves the data for audit purposes
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> DeleteListingAsync(int listingId, int userId)
        {
            //Find the seller profile for this user-ownership check
            var sellerProfile=await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            if(sellerProfile == null) return false;

            // Find the listing — must belong to this seller
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l =>
                    l.ListingId == listingId
                    && l.SellerProfileId == sellerProfile.SellerProfileId
                    && l.DeletedAt == null);

            if (listing == null)
                return false; // Listing not found or already deleted

            // Get the Deleted status ID
            var deletedStatus = await _context.ListingStatuses
                .FirstOrDefaultAsync(ls => ls.Name == "Deleted");

            // Set the soft delete fields
            listing.DeletedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;

            // Update the status to Deleted if found
            if (deletedStatus != null)
                listing.ListingStatusId = deletedStatus.ListingStatusId;

            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();

            return true; // Deletion successful
        }

        // ─────────────────────────────────────────────────────────────
        // INCREMENT VIEW COUNT
        // Increments the view count each time a listing detail page is opened
        // ─────────────────────────────────────────────────────────────
        public async Task IncrementViewCountAsync(int listingId)
        {
            // Find the listing by ID
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.ListingId == listingId && l.DeletedAt == null);

            if (listing == null)
                return; // Nothing to update if listing not found

            // Increment the view count by 1
            listing.ViewCount++;
            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — MAP TO RESPONSE DTO
        // Maps a Listing entity to a ListingResponseDto
        // Centralised here so all methods return consistent data
        // ─────────────────────────────────────────────────────────────
        private static ListingResponseDto MapToResponseDto(Listing listing)
        {
            return new ListingResponseDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                ListingType = listing.ListingType,
                IsNegotiable = listing.IsNegotiable,
                IsFeatured = listing.IsFeatured,
                Quantity = listing.Quantity,
                ViewCount = listing.ViewCount,
                CampusPickup = listing.CampusPickup,
                DeliveryAvailable = listing.DeliveryAvailable,
                CreatedAt = listing.CreatedAt,
                UpdatedAt = listing.UpdatedAt,
                ExpiresAt = listing.ExpiresAt,

                // Map related entity names — safer than returning IDs only
                Status = listing.ListingStatus?.Name ?? string.Empty,
                Condition = listing.Condition?.Name ?? string.Empty,
                CategoryId = listing.CategoryId,
                CategoryName = listing.Category?.Name ?? string.Empty,
                UniversityId = listing.UniversityId,
                UniversityName = listing.University?.Name ?? string.Empty,
                UniversityShortName = listing.University?.ShortName ?? string.Empty,
                SellerProfileId = listing.SellerProfileId,
                StoreName = listing.SellerProfile?.StoreName ?? string.Empty,
                SellerRating = listing.SellerProfile?.AverageRating ?? 0,

                // Get the cover image URL — the image marked as cover or the first one
                CoverImageUrl = listing.ListingImages?
                    .Where(i => i.IsCoverImage)
                    .Select(i => i.ImagePath)
                    .FirstOrDefault()
                    ?? listing.ListingImages?.FirstOrDefault()?.ImagePath,
            };
        }
    }
}