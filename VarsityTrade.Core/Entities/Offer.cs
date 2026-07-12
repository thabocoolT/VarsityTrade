using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Offer
    {
        public int OfferId { get; set; }
        public int ListingId { get; set; }
        public int BuyerId { get; set; }
        public int SellerProfileId { get; set; }
        public string OfferType { get; set; } = string.Empty;
        public decimal? OfferAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public Listing Listing { get; set; } = null!;
        public User Buyer { get; set; } = null!;
        public SellerProfile SellerProfile { get; set; } = null!;
        public ICollection<OfferItem> OfferItems { get; set; } = new List<OfferItem>();

    }

    
}
