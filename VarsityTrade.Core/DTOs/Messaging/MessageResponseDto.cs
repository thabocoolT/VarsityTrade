using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.DTOs.Messaging
{
    //This DTO defines what the API returns for each individual message 
    //Used inside a conversation thread
    public class MessageResponseDto
    {
        //Unique identifier for this message
        public int MessageId { get; set; }

        //The conversation this message belongs to
        public int ConversationId { get; set; }

        //Who sent this message
        public int SenderId { get; set; }
        public string SenderFirstName { get; set; } = string.Empty;
        public string SenderLastName { get; set; } = string.Empty;

        //The message content
        public string Content { get; set; } = string.Empty;

        //MessageType distinguishes between Text, Image, and System Messages
        //System messages are platform-generated e.g "Offer accepted"
        public string MessageType { get; set; } = string.Empty;

        //Whether the recipient has read this message
        public bool IsRead { get; set; }

        //When the message was sent-used for ordering in the conversation thread
        public DateTime SentAt {  get; set; }
    }
}
