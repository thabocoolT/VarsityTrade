using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;

namespace VarsityTrade.Core.Entities
{
    public class Listing
    {
        public int ListingId { get; set; }
        public int SellerProfileId { get; set; }
        public int UniversityId { get; set; }
        public int CategoryId { get; set; }
        public int ConditionId { get; set; }
        public int ListingStatusId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
    
        public string ListingType { get; set; }="Sale";
        public bool IsNegotiable { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public int Quantity { get; set; } = 1;
        public int ViewCount { get; set; } = 0;
        public bool CampusPickup {  get; set; } = true;
        public bool DeliveryAvailable { get; set; } = false;
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }


        public SellerProfile SellerProfile { get; set; } = null!;
        public University University { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Condition Condition { get; set; } = null!;
        public ListingStatus ListingStatus { get; set; } = null!;
        public ICollection<ListingImage> ListingImages { get; set; } = new List<ListingImage>();
        public ICollection<SavedListing> SavedByUsers { get; set; } = new List<SavedListing>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
        public Transaction? Transaction { get; set; }
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<HeroBannerSlide> BannerSlides { get; set; } = new List<HeroBannerSlide>();
    }
    
 
    
    
    
}
