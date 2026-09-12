namespace VarsityTrade.Web.Models.Seller
{
    // View model for the Seller Dashboard page
    public class SellerDashboardViewModel
    {
        public string StoreName { get; set; } = string.Empty;
        public string? SellerBio { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalSales { get; set; }
        public bool IsActive { get; set; }

        // Stats shown in the stats row
        public int ActiveListingsCount { get; set; }
        public int PendingOffersCount { get; set; }
        public int UnreadMessagesCount { get; set; }

        // Recent listings preview
        public List<SellerListingViewModel> RecentListings { get; set; } = new();

        // Pending offers preview
        public List<ReceivedOfferViewModel> PendingOffers { get; set; } = new();
    }

    // Represents a single listing in the seller dashboard
    public class SellerListingViewModel
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }
        public string? CoverImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Represents a single received offer in the seller dashboard
    public class ReceivedOfferViewModel
    {
        public int OfferId { get; set; }
        public string OfferType { get; set; } = string.Empty;
        public decimal? OfferAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ListingTitle { get; set; } = string.Empty;
        public string BuyerFirstName { get; set; } = string.Empty;
        public string BuyerLastName { get; set; } = string.Empty;
        public string BuyerSuburb { get; set; } = string.Empty;
        public string? BuyerResidenceName { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OfferItemDetail> OfferItems { get; set; } = new();
    }

    // A single item in a trade offer
    public class OfferItemDetail
    {
        public string Title { get; set; } = string.Empty;
        public decimal? EstimatedValue { get; set; }
    }

    // View model for the Activate Seller Profile page
    public class ActivateSellerViewModel
    {
        public string StoreName { get; set; } = string.Empty;
        public string? SellerBio { get; set; }
        public bool CampusPickup { get; set; } = true;
        public bool DeliveryAvailable { get; set; } = false;
        public bool OpenToTrades { get; set; } = true;
    }

    // View model for the Create/Edit Listing page
    public class CreateListingViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int ConditionId { get; set; }
        public string ListingType { get; set; } = "Sale";
        public bool IsNegotiable { get; set; } = false;
        public int Quantity { get; set; } = 1;
        public bool CampusPickup { get; set; } = true;
        public bool DeliveryAvailable { get; set; } = false;

        // Dropdowns
        public List<CategoryOption> Categories { get; set; } = new();
        public List<ConditionOption> Conditions { get; set; } = new();
    }

    public class CategoryOption
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class ConditionOption
    {
        public int ConditionId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}