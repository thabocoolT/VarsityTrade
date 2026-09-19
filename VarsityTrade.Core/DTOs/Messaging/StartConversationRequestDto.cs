using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices; // Provides Required validation

namespace VarsityTrade.Core.DTOs.Messaging
{
    //This DTO defines the data required to start a new conversation
    //A buyer sends this when they click "Message Seller" on a listing page

    public class StartConversationRequestDto
    {
        //The listing the buyer is inquiring about
        [Required(ErrorMessage = "Listing ID is required.")]
        public int ListingId { get; set; }

        //The first message in the conversation-required, cannot send an empty message
        [MaxLength(2000, ErrorMessage = "Message content cannot exceed 2000 characters.")]
        public string? InitialMessage { get; set; }
    }
}
