using Microsoft.EntityFrameworkCore; // Provides FirstOrDefaultAsync, ToListAsync, AnyAsync
using VarsityTrade.Core.DTOs.Notifications; // Provides Notification DTOs
using VarsityTrade.Core.Interfaces; // Provides INotificationService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext
using NotificationEntity = VarsityTrade.Core.Entities.Notification; // Resolves conflict with built-in .NET Notification type

namespace VarsityTrade.Application.Services
{
    // NotificationService handles all business logic for notification operations
    // Notifications are created by other services (OfferService, MessagingService etc.)
    // and read by the user through this service
    public class NotificationService : INotificationService
    {
        // DbContext injected for all database operations
        private readonly VarsityTradeDbContext _context;

        // Constructor receives DbContext via dependency injection
        public NotificationService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET NOTIFICATIONS
        // Returns all notifications for a user ordered newest first
        // Used on the Notifications page
        // ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<NotificationResponseDto>> GetNotificationsAsync(int userId)
        {
            // Load all notifications for this user ordered newest first
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)         // Only this user's notifications
                .OrderByDescending(n => n.CreatedAt)    // Newest notifications first
                .ToListAsync();

            return notifications.Select(n => MapToResponseDto(n));
        }

        // ─────────────────────────────────────────────────────────────
        // GET UNREAD COUNT
        // Returns the count of unread notifications for the bell badge
        // ─────────────────────────────────────────────────────────────
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            // Count notifications where ReadAt is null — meaning unread
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && n.ReadAt == null);
        }

        // ─────────────────────────────────────────────────────────────
        // MARK AS READ
        // Marks a single notification as read by setting ReadAt
        // ─────────────────────────────────────────────────────────────
        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            // Find the notification — must belong to this user
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationId == notificationId
                    && n.UserId == userId           // Ownership check
                    && n.ReadAt == null);           // Only mark if currently unread

            if (notification == null)
                return; // Already read or not found — nothing to do

            // Set ReadAt to mark as read
            notification.ReadAt = DateTime.UtcNow;
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // MARK ALL AS READ
        // Marks all unread notifications as read — bulk action
        // Called when user clicks Mark all as read on the Notifications page
        // ─────────────────────────────────────────────────────────────
        public async Task MarkAllAsReadAsync(int userId)
        {
            // Get all unread notifications for this user
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && n.ReadAt == null)
                .ToListAsync();

            if (!unread.Any())
                return; // Nothing to mark — exit early

            // Mark each notification as read
            foreach (var notification in unread)
                notification.ReadAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // CREATE NOTIFICATION
        // Creates a new notification — called internally by other services
        // e.g. OfferService calls this when an offer is accepted or rejected
        // ─────────────────────────────────────────────────────────────
        public async Task CreateNotificationAsync(
            int userId,
            string title,
            string body,
            string notificationType,
            string? actionUrl = null,
            string? imageUrl = null)
        {
            // Create the notification entity
            var notification = new NotificationEntity
            {
                UserId = userId,
                Title = title,
                Body = body,
                NotificationType = notificationType,
                ActionUrl = actionUrl,   // Optional — where to navigate on click
                ImageUrl = imageUrl,    // Optional — image shown on notification card
                ReadAt = null,        // Null means unread
                CreatedAt = DateTime.UtcNow,
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — MAP TO RESPONSE DTO
        // ─────────────────────────────────────────────────────────────
        private static NotificationResponseDto MapToResponseDto(NotificationEntity notification)
        {
            return new NotificationResponseDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Body = notification.Body,
                NotificationType = notification.NotificationType,
                ActionUrl = notification.ActionUrl,
                ImageUrl = notification.ImageUrl,
                ReadAt = notification.ReadAt,
                CreatedAt = notification.CreatedAt,
            };
        }
    }
}