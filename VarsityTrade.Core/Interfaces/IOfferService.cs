using System;
using System.Collections.Generic;
using System.Text;
using VarsityTrade.Core.DTOs.Offers; // Provides Offer DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all offer operations
    public interface IOfferService
    {
        //Get all offers made by a buyer-for the My Offers page
        Task<IEnumerable<OfferResponseDto>> GetOffersByBuyerAsync(int buyerId);

        //Get all offers received by a seller-for the Received Offers page
        Task<IEnumerable<OfferResponseDto>> GetOffersBySellerAsync(int sellerId);

        //Get a single single offer by ID
        Task<OfferResponseDto?> GetOfferByIdAsync(int offerId, int userId);

        //Create a new offer-called when a buyer submits an offer on a listing
        Task<OfferResponseDto?> CreateOfferAsync(int buyerId, OfferRequestDto request);

        // Accept an offer — seller only, creates a Transaction automatically
        Task<bool> AcceptOfferAsync(int offerId, int userId);

        // Reject an offer — seller only
        Task<bool> RejectOfferAsync(int offerId, int userId);

        // Cancel an offer — buyer only, only works on pending offers
        Task<bool> CancelOfferAsync(int offerId, int buyerId);
    }
}
