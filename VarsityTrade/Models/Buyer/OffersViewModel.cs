namespace VarsityTrade.Web.Models.Buyer
{
    // View model for the My Offers page
    public class OffersViewModel
    {
        public List<OfferViewModel> Offers { get; set; } = new();
        public string ActiveFilter { get; set; } = "All";
    }

    // Represents a single offer on the My Offers page
    public class OfferViewModel
    {
        public int OfferId { get; set; }
        public string OfferType { get; set; } = string.Empty;
        public decimal? OfferAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public decimal ListingPrice { get; set; }
        public string? ListingCoverImage { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public List<OfferItemViewModel> OfferItems { get; set; } = new();
    }

    // Represents a single trade item in an offer
    public class OfferItemViewModel
    {
        public int OfferItemId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal? EstimatedValue { get; set; }
    }
}