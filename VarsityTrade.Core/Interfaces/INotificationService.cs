using VarsityTrade.Core.DTOs.Notifications; // Provides Notification DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all notification operations
    public interface INotificationService
    {
        // Get all notifications for a user — for the Notifications page
        Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(int userId);

        // Get unread notification count — for the bell badge in the navbar
        Task<int> GetUnreadCountAsync(int userId);

        // Mark a single notification as read
        Task MarkAsReadAsync(int notificationId, int userId);

        // Mark all notifications as read — bulk action
        Task MarkAllAsReadAsync(int userId);

        // Create a notification — called internally by other services
        // e.g. when an offer is accepted the OfferService calls this
        Task CreateNotificationAsync(int userId, string title, string body,
            string notificationType, string? actionUrl = null, string? imageUrl = null);
    }
}