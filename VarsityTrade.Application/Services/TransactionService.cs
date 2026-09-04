using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore; // Provides Include, FirstOrDefaultAsync, ToListAsync
using VarsityTrade.Core.DTOs.Transactions; // Provides Transaction DTOs
using VarsityTrade.Core.Interfaces; // Provides ITransactionService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.Application.Services
{
    // TransactionService handles all business logic for transaction operations
    // Transactions are created automatically when an offer is accepted
    // They gate the review system — a review requires a completed transaction
    public class TransactionService : ITransactionService
    {
        //DbContext injected for all database operation
        private readonly VarsityTradeDbContext _context;

        //Contructor receives the DbContext via dependency injection
        public TransactionService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET TRANSACTIONS BY BUYER
        // Returns all completed transactions for a buyer — their purchase history
        // ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<TransactionResponseDto>> GetTransactionsByBuyerAsync(int buyerId)
        {
            //Load all transactions where this user is the buyer
            var transactions = await _context.Transactions
                .Include(t => t.Listing)
                .Include(t => t.Buyer)
                .Include(t => t.SellerProfile)
                .Include(t => t.Review)
                .Where(t => t.BuyerId == buyerId)
                .OrderByDescending(t => t.CompletedAt)
                .ToListAsync();

            return transactions.Select(t => MapToResponseDto(t));
        }

        //------------------------------------------------------------------------
        //GET transactions by seller
        //Returns all completed transactions for seller-their sales history
        //------------------------------------------------------------------------
        public async Task<IEnumerable<TransactionResponseDto>> GetTransactionsBySellerAsync(int userId)
        {
            // Find the seller profile for this user
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            //Return empty list if user has no seller profile
            if (sellerProfile == null)
                return Enumerable.Empty<TransactionResponseDto>();

            //Load all transactions for this seller
            var transactions = await _context.Transactions
                .Include(t => t.Listing)
                .Include(t => t.Buyer)
                .Include(t => t.SellerProfile)
                .Include(t => t.Review)
                .Include(t => t.Review)
                .Where(t => t.SellerProfileId == sellerProfile.SellerProfileId)
                .OrderByDescending(t => t.CompletedAt)
                .ToListAsync();

            return transactions.Select(t => MapToResponseDto(t));
        }
        // ─────────────────────────────────────────────────────────────
        // GET TRANSACTION BY ID
        // Returns a single transaction — user must be buyer or seller
        // ─────────────────────────────────────────────────────────────

        public async Task<TransactionResponseDto?> GetTransactionByIdAsync(int transactionId, int userId)
        {
            // Find the seller profile for this user — may be null
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            var sellerProfileId = sellerProfile?.SellerProfileId ?? 0;
            // Load the transaction — user must be buyer or seller to view it
            var transaction = await _context.Transactions
                .Include(t => t.Listing)
                .Include(t => t.Buyer)
                .Include(t => t.SellerProfile)
                .Include(t => t.Review)
                .FirstOrDefaultAsync(t =>
                    t.TransactionId == transactionId
                    && (t.BuyerId == userId || t.SellerProfileId == sellerProfileId));

            if (transaction == null)
                return null;

            return MapToResponseDto(transaction);
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — MAP TO RESPONSE DTO
        // ─────────────────────────────────────────────────────────────
        private static TransactionResponseDto MapToResponseDto(
            VarsityTrade.Core.Entities.Transaction transaction)
        {
            return new TransactionResponseDto
            {
                TransactionId = transaction.TransactionId,
                FinalPrice = transaction.FinalPrice,
                Status = transaction.Status,
                CompletedAt = transaction.CompletedAt,

                // Listing info
                ListingId = transaction.ListingId,
                ListingTitle = transaction.Listing?.Title ?? string.Empty,

                // Buyer info
                BuyerId = transaction.BuyerId,
                BuyerFirstName = transaction.Buyer?.FirstName ?? string.Empty,
                BuyerLastName = transaction.Buyer?.LastName ?? string.Empty,

                // Seller info
                SellerProfileId = transaction.SellerProfileId,
                StoreName = transaction.SellerProfile?.StoreName ?? string.Empty,

                // HasReview tells the client whether a review has already been submitted
                // This controls whether the Leave a Review button is shown
                HasReview = transaction.Review != null,
            };
        }
    }
}
