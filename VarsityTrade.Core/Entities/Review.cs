using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }
        public int TransactionId { get; set; }
        public int ReviewerId { get; set; }
        public int SellerProfileId { get; set; }
        public int Rating { get; set; } // 1 to 5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DeletedAt {  get; set; }

        public Transaction Transaction { get; set; } = null!;
        public User Reviewer { get; set; } = null!;
        public SellerProfile SellerProfile { get; set; } = null!;
        public ICollection<HeroBannerSlide> BannerSlides {  get; set; }=new List<HeroBannerSlide>();



    }
}
