using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class OfferItem
    {
        public int OfferItemId { get; set; }
        public int OfferId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal? EstimatedValue { get; set; }

        public Offer Offer { get; set; } = null!;
    }
    
}
