namespace VarsityTrade.Web.Models.Buyer
{
    // View model for the Inbox page
    public class InboxViewModel
    {
        public List<ConversationViewModel> Conversations { get; set; } = new();
        public int UnreadCount { get; set; }
    }

    // Represents a single conversation in the inbox list
    public class ConversationViewModel
    {
        public int ConversationId { get; set; }
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public decimal ListingPrice { get; set; }
        public string? ListingCoverImage { get; set; }
        public int BuyerId { get; set; }
        public string BuyerFirstName { get; set; } = string.Empty;
        public string BuyerLastName { get; set; } = string.Empty;
        public int SellerProfileId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public string? LastMessageContent { get; set; }
        public int UnreadCount { get; set; }
        public double SellerRating { get; set; }
        public string SellerFirstName { get; set; } = string.Empty;
        public string SellerLastName { get; set; } = string.Empty;
        public string SellerUniversityName { get; set; } = string.Empty;
        public string SellerUniversityShortName { get; set; } = string.Empty;
        public int SellerTotalSales { get; set; }
    }

    // Represents a single message in a conversation thread
    public class MessageViewModel
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderFirstName { get; set; } = string.Empty;
        public string SenderLastName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }

    // View model for the conversation thread page
    public class ConversationThreadViewModel
    {
        public ConversationViewModel Conversation { get; set; } = new();
        public List<MessageViewModel> Messages { get; set; } = new();
        public int CurrentUserId { get; set; }
    }
}