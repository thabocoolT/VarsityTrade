namespace VarsityTrade.Core.DTOs.Admin
{
    // This DTO defines what the API returns for each banner slide
    public class HeroBannerSlideResponseDto
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
        public bool IsVisible { get; set; }
        public int SortOrder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Listing title — shown in the slide card in the admin panel
        public string? ListingTitle { get; set; }
    }
}