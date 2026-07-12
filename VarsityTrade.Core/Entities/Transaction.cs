using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int ListingId { get; set; }
        public int BuyerId { get; set; }
        public int SellerProfileId { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; } = "Completed"; // Pending, Completed, Cancelled
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
        
        public User Buyer { get; set; } = null!;
        public Listing Listing { get; set; } = null!;
        public SellerProfile SellerProfile { get; set; } = null!;
        public Review? Review { get; set; } = null!;


    }
}
