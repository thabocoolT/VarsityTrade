using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty; // General, Transaction, Message, etc.
        public string? ActionUrl { get; set; } // Optional URL for action
        public string? ImageUrl { get; set; } // Optional image URL
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public User User { get; set; } = null!;
    }
    
    
}
