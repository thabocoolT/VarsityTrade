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
        private bool TryGetSellerMode(string? mode, out bool isSellerMode)
        {
            isSellerMode = false;

            if (string.Equals(mode, "buyer", StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals(mode, "seller", StringComparison.OrdinalIgnoreCase))
            {
                isSellerMode = true;
                return true;
            }

            return false;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/messaging/conversations
        // Returns all conversations for the logged in user
        // Used on the inbox page
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns all conversations for the logged in user — buyer and seller threads.</summary>
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(int id, [FromQuery]string? mode)
        {
            var userId = GetCurrentUserId();


            if (userId == null)
                return Unauthorized(new
                {
                    message = "User identity not found in token."
                });

            if (!TryGetSellerMode(mode, out var isSellerMode))
                return BadRequest(new
                {
                    message = "Specify mode as 'buyer' or 'seller'."
                });

            var conversations = await _messagingService.GetConversationsAsync(
                userId.Value,
                isSellerMode);

            return Ok(conversations);
        }
        // GET /api/messaging/conversations/{id}
        // Returns one conversation for the current participant
        [HttpGet("conversations/{id:int}")]
        public async Task<IActionResult> GetConversationById(
            int id,
            [FromQuery] string? mode)
        {
            var userId = GetCurrentUserId();

            if (userId == null)
            {
                return Unauthorized();
            }

            if (!TryGetSellerMode(mode, out bool isSellerMode))
            {
                return BadRequest(new
                {
                    message = "Invalid or missing messaging mode."
                });
            }

            var conversation = await _messagingService.GetConversationByIdAsync(
                id,
                userId.Value,
                isSellerMode);

            if (conversation == null)
            {
                return NotFound();
            }

            return Ok(conversation);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/messaging/conversations/{id}/messages
        // Returns all messages in a conversation — marks them as read
        // ─────────────────────────────────────────────────────────────
        /// <summary>Returns a single conversation by ID — user must be a participant.</summary>
        [HttpGet("conversations/{id}/messages")]
        public async Task<IActionResult> GetMessages(
        int id,
        [FromQuery] string? mode)
        {
            var userId = GetCurrentUserId();
            if (!TryGetSellerMode(mode, out var isSellerMode))
                return BadRequest(new
                {
                    message = "Specify mode as 'buyer' or 'seller'."
                });

            if (userId == null)
                return Unauthorized(new
                {
                    message = "User identity not found in token."
                });

            var conversation = await _messagingService.GetConversationByIdAsync(
                id,
                userId.Value,
                isSellerMode);

            if (conversation == null)
                return NotFound(new
                {
                    message = "Conversation not found."
                });

            var messages = await _messagingService.GetMessagesAsync(
                id,
                userId.Value,
                isSellerMode);

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
        [HttpPost("conversations/{id}/messages")]
        public async Task<IActionResult> SendMessage(
            int id,
            [FromQuery] string? mode,
            [FromBody] SendMessageRequestDto request)

        {
            var userId = GetCurrentUserId();
            if (!TryGetSellerMode(mode, out var isSellerMode))
                return BadRequest(new
                {
                    message = "Specify mode as 'buyer' or 'seller'."
                });
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var result = await _messagingService.SendMessageAsync(
                id,
                userId.Value,
                request,
                isSellerMode);

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
        public async Task<IActionResult> MarkAsRead(
            int id,
            [FromQuery] string? mode)
        {
            var userId = GetCurrentUserId();
            if (!TryGetSellerMode(mode, out var isSellerMode))
                return BadRequest(new
                {
                    message = "Specify mode as 'buyer' or 'seller'."
                });

            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token" });

            var conversation = await _messagingService.GetConversationByIdAsync(
             id,
             userId.Value,
             isSellerMode);

            if (conversation == null)
                return NotFound(new
                {
                    message = "Conversation not found."
                });

            await _messagingService.MarkMessagesAsReadAsync(
                id,
                userId.Value,
                isSellerMode);
            //Return 204 No content-action succeeded with no response body needed
            return NoContent();
        }

    }
}
