namespace VarsityTrade.Web.Models.Listings
{
    // View model for the Browse Listings page
    public class BrowseViewModel
    {
        // The listings to display
        public List<ListingCardViewModel> Listings { get; set; } = new();

        // The university name for the campus lock banner
        public string UniversityName { get; set; } = string.Empty;

        // Filter state — what the user has selected
        public string? SearchQuery { get; set; }
        public string? SelectedCategory { get; set; }
        public string? SelectedCondition { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        // Total count for display
        public int TotalCount => Listings.Count;
    }
}