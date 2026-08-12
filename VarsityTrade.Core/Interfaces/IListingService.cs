using System;
using System.Collections.Generic;
using System.Text;
using VarsityTrade.Core.DTOs.Listings; // Provides Listing DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all listing operations
    // The controller depends on this interface — not the concrete service
    public interface IListingService
    {
        // Get all active listings for a specific university — the main campus-locked feed
        Task<IEnumerable<ListingResponseDto>> GetListingsByUniversityAsync(int universityId);

        // Get a single listing by its ID — for the listing detail page
        Task<ListingResponseDto?> GetListingByIdAsync(int listingId);

        // Create a new listing — takes userId and looks up the seller profile internally
        Task<ListingResponseDto?> CreateListingAsync(int userId, ListingRequestDto request);

        // Update an existing listing — only the owner can update it
        Task<ListingResponseDto?> UpdateListingAsync(int listingId, int userId, ListingRequestDto request);

        // Soft delete a listing — sets DeletedAt and status to Deleted
        Task<bool> DeleteListingAsync(int listingId, int userId);

        // Increment the view count each time a listing detail page is opened
        Task IncrementViewCountAsync(int listingId);
    }
}