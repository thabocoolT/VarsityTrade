using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using System.Security.Claims; // Provides ClaimTypes for reading JWT claims
using VarsityTrade.Core.Interfaces; // Provides INotificationService

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // All routes start with /api/notifications
    [Authorize] // All notification endpoints require authentication
    public class NotificationsController : ControllerBase
    {
        // INotificationService injected — controller stays thin
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        // Helper to read user ID from JWT token
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/notifications
        // Returns all notifications for the logged in user
        // Used on the Notifications page
        // ─────────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var notifications = await _notificationService.GetNotificationsAsync(userId.Value);
            return Ok(notifications);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/notifications/unread-count
        // Returns the count of unread notifications — for the bell badge
        // ─────────────────────────────────────────────────────────────
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var count = await _notificationService.GetUnreadCountAsync(userId.Value);
            return Ok(new { unreadCount = count });
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/notifications/{id}/read
        // Marks a single notification as read
        // ─────────────────────────────────────────────────────────────
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            await _notificationService.MarkAsReadAsync(id, userId.Value);
            return NoContent(); // 204 — success with no response body
        }

        // ─────────────────────────────────────────────────────────────
        // PUT /api/notifications/read-all
        // Marks all notifications as read — bulk action
        // ─────────────────────────────────────────────────────────────
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            await _notificationService.MarkAllAsReadAsync(userId.Value);
            return NoContent(); // 204 — success with no response body
        }
    }
}