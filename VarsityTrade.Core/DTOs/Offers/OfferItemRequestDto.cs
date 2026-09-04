using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; // Provides Required and Range validation

namespace VarsityTrade.Core.DTOs.Offers
{
    // This DTO defines a single item included in a trade offer
    // A buyer can offer multiple items as part of one trade
    public class OfferItemRequestDto
    {
        // Title describes what the buyer is offering — required
        [Required(ErrorMessage = "Item title is required")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
        public string Title { get; set; } = string.Empty;

        // EstimatedValue is the buyer's self-assessed value of the item — optional
        [Range(0, double.MaxValue, ErrorMessage = "Estimated value must be zero or greater")]
        public decimal? EstimatedValue { get; set; }
    }
}