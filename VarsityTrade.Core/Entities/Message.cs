using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Message
    {
        public int MessageId { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string MessageType { get; set; } = "Text"; // Text, Image, File, etc.
        public bool IsRead { get; set; } = false;
        public bool DeletedBySender { get; set; } = false;
        public bool DeletedByReceiver { get; set; } = false;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;


        public Conversation Conversation { get; set; } = null!;
        public User Sender { get; set; } = null!;
    }
    
    
}
