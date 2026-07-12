using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class SellerProfile
    {
        public int SellerProfileId { get; set; }
        public int UserId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? SellerBio { get; set; }
        public bool CampusPickup { get; set; } = true;
        public bool DeliveryAvailable { get; set; } = false;
        public bool OpenToTrade { get; set; } = false;
        public decimal AverageRating { get; set; } = 0.00m;
        public int TotalSales { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }

        public User User { get; set; } = null!;
        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
    
    
}
