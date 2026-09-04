using Microsoft.EntityFrameworkCore; // Provides Include, FirstOrDefaultAsync, ToListAsync
using VarsityTrade.Core.DTOs.Offers; // Provides Offer DTOs
using VarsityTrade.Core.Entities; // Provides Offer, OfferItem, Transaction entities
using VarsityTrade.Core.Interfaces; // Provides IOfferService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.Application.Services
{
    public class OfferService : IOfferService
    {
        // DbContext injected for all database operations
        private readonly VarsityTradeDbContext _context;

        // Constructor receives the DbContext via dependency injection
        public OfferService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET OFFERS BY BUYER
        // Returns all offers made by a specific buyer
        // Used on the My Offers page in the buyer dashboard
        // ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<OfferResponseDto>> GetOffersByBuyerAsync(int buyerId)
        {
            // Load all offers made by this buyer with all related data
            var offers = await _context.Offers
                .Include(o => o.Listing)              // Load listing for title and price
                    .ThenInclude(l => l.ListingImages)       // Load images for cover photo
                .Include(o => o.Buyer)                // Load buyer info
                    .ThenInclude(u => u.Location)     // Load buyer location for pickup details
                .Include(o => o.SellerProfile)        // Load seller profile for store name
                .Include(o => o.OfferItems)           // Load trade items included in the offer
                .Where(o => o.BuyerId == buyerId)     // Filter to this buyer's offers only
                .OrderByDescending(o => o.CreatedAt)  // Newest offers first
                .ToListAsync();

            return offers.Select(o => MapToResponseDto(o));
        }

        // ─────────────────────────────────────────────────────────────
        // GET OFFERS BY SELLER
        // Returns all offers received by a seller
        // Used on the Received Offers page in the seller dashboard
        // ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<OfferResponseDto>> GetOffersBySellerAsync(int userId)
        {
            // Find the seller profile for this user
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            // Return empty list if user has no seller profile
            if (sellerProfile == null)
                return Enumerable.Empty<OfferResponseDto>();

            // Load all offers received by this seller
            var offers = await _context.Offers
                .Include(o => o.Listing)
                    .ThenInclude(l => l.ListingImages)
                .Include(o => o.Buyer)
                    .ThenInclude(u => u.Location)
                .Include(o => o.SellerProfile)
                .Include(o => o.OfferItems)
                .Where(o => o.SellerProfileId == sellerProfile.SellerProfileId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return offers.Select(o => MapToResponseDto(o));
        }

        // ─────────────────────────────────────────────────────────────
        // GET OFFER BY ID
        // Returns a single offer — user must be the buyer or the seller
        // ─────────────────────────────────────────────────────────────
        public async Task<OfferResponseDto?> GetOfferByIdAsync(int offerId, int userId)
        {
            // Find the seller profile for this user — may be null
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            var sellerProfileId = sellerProfile?.SellerProfileId ?? 0;

            // Load the offer — user must be buyer or seller to view it
            var offer = await _context.Offers
                .Include(o => o.Listing)
                    .ThenInclude(l => l.ListingImages)
                .Include(o => o.Buyer)
                    .ThenInclude(u => u.Location)
                .Include(o => o.SellerProfile)
                .Include(o => o.OfferItems)
                .FirstOrDefaultAsync(o =>
                    o.OfferId == offerId
                    && (o.BuyerId == userId || o.SellerProfileId == sellerProfileId));

            if (offer == null)
                return null;

            return MapToResponseDto(offer);
        }

        // ─────────────────────────────────────────────────────────────
        // CREATE OFFER
        // Creates a new offer on a listing
        // Called when a buyer submits an offer from the listing detail page
        // ─────────────────────────────────────────────────────────────
        public async Task<OfferResponseDto?> CreateOfferAsync(int buyerId, OfferRequestDto request)
        {
            // Load the listing to get the seller profile ID
            var listing = await _context.Listings
                .Include(l => l.SellerProfile) // Load seller profile for ownership check
                .FirstOrDefaultAsync(l =>
                    l.ListingId == request.ListingId
                    && l.DeletedAt == null);

            // Return null if listing not found or deleted
            if (listing == null)
                return null;

            // Prevent sellers from making offers on their own listings
            if (listing.SellerProfile.UserId == buyerId)
                return null;

            // Create the offer entity
            var offer = new Offer
            {
                ListingId = request.ListingId,
                BuyerId = buyerId,
                SellerProfileId = listing.SellerProfile.SellerProfileId,
                OfferType = request.OfferType,
                OfferAmount = request.OfferAmount,
                Status = "Pending", // All new offers start as Pending
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Offers.AddAsync(offer);
            await _context.SaveChangesAsync(); // Save to get the OfferId

            // Create the offer items if any were included
            if (request.OfferItems.Any())
            {
                var offerItems = request.OfferItems.Select(item => new OfferItem
                {
                    OfferId = offer.OfferId,  // Link to the offer we just created
                    Title = item.Title,
                    EstimatedValue = item.EstimatedValue,
                }).ToList();

                await _context.OfferItems.AddRangeAsync(offerItems);
                await _context.SaveChangesAsync();
            }

            // Reload with all related data for the response
            return await GetOfferByIdAsync(offer.OfferId, buyerId);
        }

        // ─────────────────────────────────────────────────────────────
        // ACCEPT OFFER
        // Seller accepts an offer — creates a Transaction automatically
        // Also rejects all other pending offers on the same listing
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> AcceptOfferAsync(int offerId, int userId)
        {
            // Find the seller profile for this user
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            if (sellerProfile == null)
                return false;

            // Find the offer — must belong to this seller and be pending
            var offer = await _context.Offers
                .Include(o => o.Listing) // Load listing to update its status
                .FirstOrDefaultAsync(o =>
                    o.OfferId == offerId
                    && o.SellerProfileId == sellerProfile.SellerProfileId
                    && o.Status == "Pending"); // Can only accept pending offers

            if (offer == null)
                return false;

            // Update the offer status to Accepted
            offer.Status = "Accepted";
            offer.UpdatedAt = DateTime.UtcNow;
            _context.Offers.Update(offer);

            // Get the Reserved status for the listing
            var reservedStatus = await _context.ListingStatuses
                .FirstOrDefaultAsync(ls => ls.Name == "Reserved");

            // Update the listing status to Reserved — item is pending collection
            if (reservedStatus != null && offer.Listing != null)
            {
                offer.Listing.ListingStatusId = reservedStatus.ListingStatusId;
                offer.Listing.UpdatedAt = DateTime.UtcNow;
                _context.Listings.Update(offer.Listing);
            }

            // Reject all other pending offers on this listing
            // A listing can only be sold to one buyer
            var otherOffers = await _context.Offers
                .Where(o =>
                    o.ListingId == offer.ListingId
                    && o.OfferId != offerId           // Exclude the accepted offer
                    && o.Status == "Pending")         // Only reject pending ones
                .ToListAsync();

            foreach (var otherOffer in otherOffers)
            {
                otherOffer.Status = "Rejected"; // Auto-reject competing offers
                otherOffer.UpdatedAt = DateTime.UtcNow;
            }

            // Create a Transaction record to mark the sale as complete
            var transaction = new Transaction
            {
                ListingId = offer.ListingId,
                BuyerId = offer.BuyerId,
                SellerProfileId = sellerProfile.SellerProfileId,
                FinalPrice = offer.OfferAmount ?? offer.Listing?.Price ?? 0,
                Status = "Completed",
                CompletedAt = DateTime.UtcNow,
            };

            await _context.Transactions.AddAsync(transaction);

            // Increment the seller's total sales count
            sellerProfile.TotalSales++;
            _context.SellerProfiles.Update(sellerProfile);

            await _context.SaveChangesAsync();
            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // REJECT OFFER
        // Seller rejects an offer — offer status updated to Rejected
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> RejectOfferAsync(int offerId, int userId)
        {
            // Find the seller profile for this user
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            if (sellerProfile == null)
                return false;

            // Find the offer — must belong to this seller and be pending
            var offer = await _context.Offers
                .FirstOrDefaultAsync(o =>
                    o.OfferId == offerId
                    && o.SellerProfileId == sellerProfile.SellerProfileId
                    && o.Status == "Pending");

            if (offer == null)
                return false;

            // Update the offer status to Rejected
            offer.Status = "Rejected";
            offer.UpdatedAt = DateTime.UtcNow;

            _context.Offers.Update(offer);
            await _context.SaveChangesAsync();

            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // CANCEL OFFER
        // Buyer cancels their own pending offer
        // ─────────────────────────────────────────────────────────────
        public async Task<bool> CancelOfferAsync(int offerId, int buyerId)
        {
            // Find the offer — must belong to this buyer and be pending
            // Buyers can only cancel offers they made and that are still pending
            var offer = await _context.Offers
                .FirstOrDefaultAsync(o =>
                    o.OfferId == offerId
                    && o.BuyerId == buyerId
                    && o.Status == "Pending");

            if (offer == null)
                return false;

            // Update the offer status to Cancelled
            offer.Status = "Cancelled";
            offer.UpdatedAt = DateTime.UtcNow;

            _context.Offers.Update(offer);
            await _context.SaveChangesAsync();

            return true;
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — MAP TO RESPONSE DTO
        // ─────────────────────────────────────────────────────────────
        private static OfferResponseDto MapToResponseDto(Offer offer)
        {
            return new OfferResponseDto
            {
                OfferId = offer.OfferId,
                OfferType = offer.OfferType,
                OfferAmount = offer.OfferAmount,
                Status = offer.Status,
                CreatedAt = offer.CreatedAt,
                UpdatedAt = offer.UpdatedAt,

                // Listing info
                ListingId = offer.ListingId,
                ListingTitle = offer.Listing?.Title ?? string.Empty,
                ListingPrice = offer.Listing?.Price ?? 0,
                ListingCoverImage = offer.Listing?.ListingImages?
                    .Where(i => i.IsCoverImage)
                    .Select(i => i.ImagePath)
                    .FirstOrDefault()
                    ?? offer.Listing?.ListingImages?.FirstOrDefault()?.ImagePath,

                // Buyer info — includes location for pickup coordination
                BuyerId = offer.BuyerId,
                BuyerFirstName = offer.Buyer?.FirstName ?? string.Empty,
                BuyerLastName = offer.Buyer?.LastName ?? string.Empty,
                BuyerSuburb = offer.Buyer?.Location?.Suburb ?? string.Empty,
                BuyerResidenceName = offer.Buyer?.Location?.ResidenceName,

                // Seller info
                SellerProfileId = offer.SellerProfileId,
                StoreName = offer.SellerProfile?.StoreName ?? string.Empty,

                // Trade items included in this offer
                OfferItems = offer.OfferItems?.Select(item => new OfferItemResponseDto
                {
                    OfferItemId = item.OfferItemId,
                    OfferId = item.OfferId,
                    Title = item.Title,
                    EstimatedValue = item.EstimatedValue,
                }).ToList() ?? new List<OfferItemResponseDto>(),
            };
        }
    }
}