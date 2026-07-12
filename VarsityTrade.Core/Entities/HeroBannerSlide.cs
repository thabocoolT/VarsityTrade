using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class HeroBannerSlide
    {
        public int HeroBannerSlideId { get; set; }
        public string SlideType { get; set; } = string.Empty;
        public int? ListingId { get; set; }
        public int? ReviewId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? ImageUrl { get; set; }
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }
        public bool IsVisible { get; set; } = true;
        public int SortOrder { get; set; } = 0;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }


        public Listing? Listing { get; set; }
        public Review? Review { get; set; }
    }
}
