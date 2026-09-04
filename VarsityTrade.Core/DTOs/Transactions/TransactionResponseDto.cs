using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.DTOs.Transactions
{
    // This DTO defines what the API returns when a transaction is requested
    // A transaction is created when a seller accepts an offer
    // It gates the review system — no transaction means no review
    public class TransactionResponseDto
    {
        public int TransactionId { get; set; }
        public decimal FinalPrice {  get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }

        // Listing info — what was sold
        public int ListingId { get; set; }
        public string ListingTitle { get; set; }= string.Empty;

        // Buyer info
        public int BuyerId { get; set; }
        public string BuyerFirstName { get; set; } = string.Empty;
        public string BuyerLastName { get; set; } = string.Empty;

        //Seller info
        public int SellerProfileId { get; set; }
        public string StoreName {  get; set; } = string.Empty;

        // Whether a review has already been left for this transaction
        // Used to show or hide the Leave a Review button on the buyer's side
        public bool HasReview { get; set; }
    }
}
