namespace VarsityTrade.Web.Models.Listings
{
    public class BrowseViewModel
    {
        public List<ListingCardViewModel> Listings { get; set; }
            = new();

        public string UniversityName { get; set; }
            = string.Empty;

        public string? SearchQuery { get; set; }

        public string? SelectedCategory { get; set; }

        public string? SelectedCondition { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public decimal? SelectedMinRating { get; set; }

        public string? SelectedSort { get; set; }

        public List<string> Categories { get; set; }
            = new();

        public int TotalCount =>
            Listings.Count;
    }
}