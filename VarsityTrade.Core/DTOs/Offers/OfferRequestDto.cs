using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; // Provides Required and Range validation

namespace VarsityTrade.Core.DTOs.Offers
{
    // This DTO defines the data a buyer must provide when making an offer
    // Supports cash offers, trade offers, and combined cash and trade offers
    public class OfferRequestDto
    {
        //The listing the buyer wants make an offer on
        [Required(ErrorMessage ="Listing ID is required")]
        public int ListingId { get; set; }

        // OfferType must be Cash, Trade, or Cash and Trade
        [Required(ErrorMessage = "Offer type is required")]
        public string OfferType { get; set; } = string.Empty;

        //OfferAmount is the component-required for cash and trade offers
        //Optional for pure Trade offers where no cash is involved
        [Range(0, double.MaxValue, ErrorMessage ="Offer amount must be zero or greater")]
        public decimal? OfferAmount { get; set; }

        //OfferItems are the trade items-required for Trade and Cash and Trade offers
        //Each item has a title and an optional estimated value
        public List<OfferItemRequestDto> OfferItems {  get; set; }=new List<OfferItemRequestDto>();
     
    }
}
