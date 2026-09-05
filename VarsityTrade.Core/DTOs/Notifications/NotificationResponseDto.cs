namespace VarsityTrade.Core.DTOs.Notifications
{
    // This DTO defines what the API returns for each notification
    // Used on the Notifications page and in the notification bell dropdown
    public class NotificationResponseDto
    {
        // Core notification identity
        public int NotificationId { get; set; }

        // Title and body are the notification text shown to the user
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;

        // NotificationType controls which icon and colour is shown
        // Values: NewMessage, OfferReceived, OfferAccepted, OfferRejected,
        //         PriceDrop, ReviewReminder, System
        public string NotificationType { get; set; } = string.Empty;

        // ActionUrl is where the user is taken when they click the notification
        // e.g. /messaging/conversations/5 or /offers/12
        public string? ActionUrl { get; set; }

        // ImageUrl is an optional image shown on the notification card
        public string? ImageUrl { get; set; }

        // ReadAt is null if unread — used to show the unread badge
        public DateTime? ReadAt { get; set; }

        // CreatedAt is used for sorting notifications newest first
        public DateTime CreatedAt { get; set; }

        // IsRead is a convenience property derived from ReadAt
        public bool IsRead => ReadAt.HasValue;
    }
}