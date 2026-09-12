namespace VarsityTrade.Web.Models.Buyer
{
    // View model for the Notifications page
    public class NotificationsViewModel
    {
        public List<NotificationViewModel> Notifications { get; set; } = new();
        public int UnreadCount { get; set; }
    }

    // Represents a single notification
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string NotificationType { get; set; } = string.Empty;
        public string? ActionUrl { get; set; }
        public DateTime? ReadAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead => ReadAt.HasValue;
    }
}