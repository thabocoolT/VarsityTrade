using System.ComponentModel.DataAnnotations; // Provides Required and MaxLength validation

namespace VarsityTrade.Core.DTOs.Admin
{
    // This DTO defines the data admin provides when creating or updating a banner slide
    public class HeroBannerSlideRequestDto
    {
        // SlideType must be one of the four supported types
        [Required(ErrorMessage = "Slide type is required")]
        public string SlideType { get; set; } = string.Empty;

        // ListingId is used for FeaturedListing and RecentPurchase slides
        public int? ListingId { get; set; }

        // ReviewId is used for RecentReview slides
        public int? ReviewId { get; set; }

        // Title is shown as the main headline on the slide
        [Required(ErrorMessage = "Title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        // Subtitle is shown below the title
        [MaxLength(500, ErrorMessage = "Subtitle cannot exceed 500 characters")]
        public string? Subtitle { get; set; }

        // ImageUrl is the background or featured image on the slide
        public string? ImageUrl { get; set; }

        // ButtonText and ButtonUrl control the CTA button on the slide
        public string? ButtonText { get; set; }
        public string? ButtonUrl { get; set; }

        // IsVisible controls whether the slide shows on the home page
        public bool IsVisible { get; set; } = true;

        // SortOrder controls the position of the slide in the rotation
        public int SortOrder { get; set; } = 0;

        // StartDate and EndDate allow time-limited slides
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}