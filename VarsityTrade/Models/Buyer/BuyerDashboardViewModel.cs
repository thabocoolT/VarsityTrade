using VarsityTrade.Web.Models.Listings; // Provides ListingCardViewModel

namespace VarsityTrade.Web.Models.Buyer
{
    // View model for the Buyer Dashboard page
    public class BuyerDashboardViewModel
    {
        //User info shown in the welcome bar
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UniversityName { get; set; } = string.Empty;


        //Stats shown in the stats row
        public int SavedListingsCount { get; set; }
        public int ActiveOffersCount { get; set; }
        public int UnreadMessagesCount { get; set; }
        public int UnreadNotificationsCount { get; set; }

        //Recent saved listings preview
        public List<ListingCardViewModel> RecentSavedListings { get; set; } = new();

        //Recent offers preview
        public List<OfferSummaryViewModel> RecentOffers { get; set; } = new();
    }

    //Summary of a single offer for dashboard preview
    public class  OfferSummaryViewModel
    {
        public int OfferId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public decimal? OfferAmount { get; set; }
        public string OfferType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
    
}
