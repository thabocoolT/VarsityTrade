using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using System.Security.Claims; // Provides ClaimTypes for reading JWT claims
using VarsityTrade.Core.DTOs.Messaging; // Provides Messaging DTOs
using VarsityTrade.Core.Entities;
using VarsityTrade.Core.Interfaces; // Provides IMessagingService


namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]//All routes start with /api/messaging
    [Authorize]//All messaging endpoints require authentication
    public class MessagingController : ControllerBase
    {
        //IMessagingService injected-controller stays thin
        private readonly IMessagingService _messagingService;

        public MessagingController(IMessagingService messagingService)
        {
            _messagingService = messagingService;
        }

        //Helper methid to read the user ID from the JWT token
        //Cebtralized here to avoid repeating the same code in every action

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/messaging/conversations
        // Returns all conversations for the logged in user
        // Used on the inbox page
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns all conversations for the logged in user — buyer and seller threads.</summary>
        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var conversation = await _messagingService.GetConversationsAsync(userId.Value);

            if (conversation == null)
                return NotFound(new { message = "Conversation not found." });

            return Ok(conversation);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/messaging/conversations/{id}/messages
        // Returns all messages in a conversation — marks them as read
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns a single conversation by ID — user must be a participant.</summary>
        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var messages = await _messagingService.GetMessagesAsync(id, userId.Value);
            return Ok(messages);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/messaging/conversations
        // Starts a new conversation — buyer clicks Message Seller
        // Returns existing conversation if one already exists
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns all messages in a conversation and marks them as read.</summary>
        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation(
            [FromBody] StartConversationRequestDto request)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token" });

            var result = await _messagingService.StartConversationAsync(userId.Value, request);
            if (result == null)
                return BadRequest(new { message = "Could not start conversation. Listing may not exist or you may be the seller" });

            return CreatedAtAction(
                nameof(GetConversations),
                new { id = result.ConversationId },
                result);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/messaging/conversations/{id}/messages
        // Sends a new message in an existing conversation
        // ─────────────────────────────────────────────────────────────
        /// <summary>Starts a new conversation or returns the existing one for this listing and buyer.</summary>
        [HttpPost("conversation/{id}")]
        public async Task<IActionResult> SendMessage(
            int id,
            [FromBody] SendMessageRequestDto request)

        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var result = await _messagingService.SendMessageAsync(id, userId.Value, request);

            if (result == null)
                return BadRequest(new { message = "Could not send message. You may not be a participant in this conversation." });


            return Ok(result);
        }
        //-------------------------------------------------------------------------------
        //PUT/api/messaging/conversations/{id}/read
        //Marks all messages ina conversation as read
        //Called when a user opens a conversation thread
        //-------------------------------------------------------------------------------
        /// <summary>Marks all messages in a conversation as read.</summary>
        [HttpPut("conversations/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token" });

            await _messagingService.MarkMessagesAsReadAsync(id, userId.Value);
            //Return 204 No content-action succeeded with no response body needed
            return NoContent();
        }

    }
}
