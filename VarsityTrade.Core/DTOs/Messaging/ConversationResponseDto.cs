using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.DTOs.Messaging
{
    //This DTO defines what the API returns when a conversation is requested
    //It is shown on the inbox page-one card per conversation
    public class ConversationResponseDto
    {
        //Unique identifier for the conversation
        public int ConversationId { get; set; }

        //The Listing this conversation is about
        public int ListingId { get; set; }
        public string ListingTitle { get; set; } = string.Empty;
        public decimal ListingPrice { get; set; }
        public string? ListingCoverImage {  get; set; }

        //The buyer in this conversation
        public int BuyerId { get; set; }
        public string BuyerFirstName { get; set; }= string.Empty;
        public string BuyerLastName { get; set; }= string.Empty;

        //The seller in this conversation
        public int SellerProfileId { get; set; }
        public string StoreName { get; set; } = string.Empty;

        //Timestamps for sorting the inbox by most recent activity
        public DateTime CreatedAt { get; set; }
        public DateTime? LastMessageAt { get; set; }

        //The last message preview shown on the inbox card
        public string? LastMessageContent { get; set; }

        //Unread message count--used to show a badge on the inbox card
        public int UnreadCount { get; set; }
    }
}
