namespace VarsityTrade.Core.DTOs.Offers
{
    // This DTO defines what the API returns for each item in a trade offer
    public class OfferItemResponseDto
    {
        public int OfferItemId { get; set; }
        public int OfferId { get; set; }

        // What the buyer is offering to trade
        public string Title { get; set; } = string.Empty;

        // The buyer's estimated value of this item
        public decimal? EstimatedValue { get; set; }
    }
}