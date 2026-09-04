using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.DTOs.Offers
{
    //This DTO defines what the API returns when an offer is requested
    //Used on the Offers page(buyer) and Received Offers page(seller)
    public class OfferResponseDto
    {
        // Core offer identity
        public int OfferId { get; set; }
        public string OfferType { get; set; } = string.Empty;
        public decimal? OfferAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        //Listing info-shown on the offer card
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public decimal ListingPrice { get; set; }
        public string? ListingCoverImage { get; set; }

        //Buyer info-shown on the seller's received offers page
        public int BuyerId { get; set; }
        public string BuyerFirstName { get; set; } = string.Empty;
        public string BuyerLastName { get; set; } = string.Empty;
        public string BuyerSuburb { get; set; } = string.Empty;
        public string? BuyerResidenceName { get; set; }

        //Seller info-shown on the buyer's my offers page
        public int SellerProfileId { get; set; }
        public string StoreName {  get; set; } = string.Empty;


        //The trade items included in this offer-empty for cash-only offers
        public List<OfferItemResponseDto> OfferItems { get; set; }= new List<OfferItemResponseDto>();
    }
}
