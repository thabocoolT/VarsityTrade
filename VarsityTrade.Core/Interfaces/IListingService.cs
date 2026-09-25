using VarsityTrade.Core.DTOs.Listings;

namespace VarsityTrade.Core.Interfaces
{
    public interface IListingService
    {
        Task<IEnumerable<ListingResponseDto>>
            GetListingsByUniversityAsync(
                int universityId);

        Task<IEnumerable<ListingResponseDto>>
            GetAllActiveListingsAsync();

        Task<ListingResponseDto?>
            GetListingByIdAsync(
                int listingId);

        Task<ListingResponseDto?>
            CreateListingAsync(
                int userId,
                ListingRequestDto request);

        Task<ListingResponseDto?>
            UpdateListingAsync(
                int listingId,
                int userId,
                ListingRequestDto request);

        Task<bool>
            DeleteListingAsync(
                int listingId,
                int userId);

        Task
            IncrementViewCountAsync(
                int listingId);

        Task<bool> SaveListingAsync(int userId, int listingId);

        Task<bool> UnsaveListingAsync(int userId, int listingId);

        Task<bool> IsListingSavedAsync(int userId, int listingId);

        Task<IEnumerable<ListingResponseDto>> GetSavedListingsAsync(int userId);
    }
}