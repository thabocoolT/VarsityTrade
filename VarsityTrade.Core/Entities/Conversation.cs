using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Conversation
    {
        public int ConversationId { get; set; }
        public int ListingId { get; set; }
        public int BuyerId { get; set; }
        public int SellerProfileId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastMessageAt {  get; set; }
        public DateTime? DeletedAt {  get; set; }

        public Listing Listing { get; set; } = null!;
        public User Buyer { get; set; } = null!;
        public SellerProfile SellerProfile { get; set; } = null!;
        public ICollection<Message> Messages { get; set; } = new List<Message>();

    }
}
