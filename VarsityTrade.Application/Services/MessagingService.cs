using Microsoft.EntityFrameworkCore; // Provides Include, FirstOrDefaultAsync, ToListAsync
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using VarsityTrade.Core.DTOs.Messaging; // Provides Messaging DTOs
using VarsityTrade.Core.Entities; // Provides Conversation and Message entities
using VarsityTrade.Core.Interfaces; // Provides IMessagingService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.Application.Services
{
    //MessagingService handles all business logi for conversations and messages
    //It enforces ownership rules and ensures that users can only access their own conversations and messages
    public class MessagingService : IMessagingService
    {
        //DbContext injected for all database operations related to messaging
        private readonly VarsityTradeDbContext _context;

        //Constructor receives the DbContext via dependency injection
        public MessagingService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        //--------------------------------------------------------------------
        //GET Conversations
        //Returns all conversations for a user-both as buyer and as seller
        //---------------------------------------------------------------------
        public async Task<IEnumerable<ConversationResponseDto>> GetConversationsAsync(
        int userId,
        bool isSellerMode)
        {
            // Load the active seller profile when seller mode is requested.
            int? sellerProfileId = null;

            if (isSellerMode)
            {
                sellerProfileId = await _context.SellerProfiles
                    .Where(sp => sp.UserId == userId && sp.IsActive)
                    .Select(sp => (int?)sp.SellerProfileId)
                    .FirstOrDefaultAsync();

                // Seller mode requires an active seller profile.
                if (sellerProfileId == null)
                    return Enumerable.Empty<ConversationResponseDto>();
            }

            var conversations = await _context.Conversations
                .Include(c => c.Listing)
                    .ThenInclude(l => l.ListingImages)
                .Include(c => c.Buyer)
                    .ThenInclude(b => b.University)
                .Include(c => c.SellerProfile)
                    .ThenInclude(sp => sp.User)
                        .ThenInclude(u => u.University)
                .Include(c => c.Messages)
                .Where(c =>
                    !isSellerMode
                        ? c.BuyerId == userId
                        : c.SellerProfileId == sellerProfileId)
                .Where(c => c.DeletedAt == null)
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt)
                .ToListAsync();

            return conversations.Select(c => MapToConversationDto(c, userId));
        }
        //----------------------------------------------------------------
        //GET Conversation by ID
        //Returns a single conversation by ID for the conversation page
        //----------------------------------------------------------------

        public async Task<ConversationResponseDto?> GetConversationByIdAsync(
    int conversationId,
    int userId,
    bool isSellerMode)
        {
            int? sellerProfileId = null;

            if (isSellerMode)
            {
                sellerProfileId = await _context.SellerProfiles
                    .Where(sp => sp.UserId == userId && sp.IsActive)
                    .Select(sp => (int?)sp.SellerProfileId)
                    .FirstOrDefaultAsync();

                if (sellerProfileId == null)
                    return null;
            }

            var conversation = await _context.Conversations
                .Include(c => c.Listing)
                    .ThenInclude(l => l.ListingImages)
                .Include(c => c.Buyer)
                    .ThenInclude(b => b.University)
                .Include(c => c.SellerProfile)
                    .ThenInclude(sp => sp.User)
                        .ThenInclude(u => u.University)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c =>
                    c.ConversationId == conversationId
                    && c.DeletedAt == null
                    && (
                        !isSellerMode
                            ? c.BuyerId == userId
                            : c.SellerProfileId == sellerProfileId
                    ));

            if (conversation == null)
                return null;

            return MapToConversationDto(conversation, userId);
        }
        //----------------------------------------------------------------
        //GET Messages in a Conversation
        //Returns all messages in a conversation-ordered oldest to newest
        //----------------------------------------------------------------
        public async Task<IEnumerable<MessageResponseDto>> GetMessagesAsync(
    int conversationId,
    int userId,
    bool isSellerMode)
        {
            // Confirm that this conversation belongs to the active profile mode.
            var conversation = await GetConversationByIdAsync(
                conversationId,
                userId,
                isSellerMode);

            if (conversation == null)
                return Enumerable.Empty<MessageResponseDto>();

            var messages = await _context.Messages
                .Include(m => m.Sender)
                .Where(m =>
                    m.ConversationId == conversationId
                    && !m.DeletedBySender
                    && !m.DeletedByReceiver)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            await MarkMessagesAsReadAsync(
                conversationId,
                userId,
                isSellerMode);

            return messages.Select(m => MapToMessageDto(m));
        }
        //----------------------------------------------------------------
        //Start a conversation
        //Creates a new conversation or return the sexisting one
        //Called when a buyer clicks Message Seller on a listing
        //----------------------------------------------------------------
        public async Task<ConversationResponseDto?>StartConversationAsync(int buyerId, StartConversationRequestDto request)
        {
            //Load the listing to get the seller profile ID
            var listing =await _context.Listings
                .Include(l=>l.SellerProfile) //Load seller profile for name and profile picture
                .FirstOrDefaultAsync(l=>
                    l.ListingId == request.ListingId
                    && l.DeletedAt == null//Exclude soft-deleted listings
                );

            //Return null if listing not found or deleted
            if(listing == null)
                return null;

            //Prevent sellers from messaging themselves on their own listing
            if(listing.SellerProfile.UserId == buyerId)
                return null;

            //Check if a conversation already exists between this buyer and listing
            //The unique constraint  UQ_Conversation_BuyerListing enforces one thread per pair
            var existing=await _context.Conversations
                .Include(c=>c.Listing)
                    .ThenInclude(l=> l.ListingImages)
                .Include(c => c.Buyer)
                    .ThenInclude(b => b.University)
                .Include(c => c.SellerProfile)
                    .ThenInclude(sp => sp.User)
                        .ThenInclude(u => u.University)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c=>
                    c.BuyerId == buyerId
                    && c.ListingId == request.ListingId
                    && c.DeletedAt == null);

            //If conversation already exists just send the new message into it
            if (existing != null)
            {
                return MapToConversationDto(existing, buyerId);
            }
            // Create a new conversation
            var conversation = new Conversation
            {
                ListingId = request.ListingId,
                BuyerId = buyerId,
                SellerProfileId = listing.SellerProfile.SellerProfileId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow, // Set to now since the first message is sent
            };
            await _context.Conversations.AddAsync(conversation);
            await _context.SaveChangesAsync(); //Save to get conevrsation ID

            // Only create the first message if the caller supplied one.
            if (!string.IsNullOrWhiteSpace(request.InitialMessage))
            {
                var initialMessage = new Message
                {
                    ConversationId = conversation.ConversationId,
                    SenderId = buyerId,
                    Content = request.InitialMessage,
                    MessageType = "Text",
                    IsRead = false,
                    SentAt = DateTime.UtcNow,
                };

                await _context.Messages.AddAsync(initialMessage);

                conversation.LastMessageAt = initialMessage.SentAt;

                await _context.SaveChangesAsync();
            }

            // Reload with all related data for the response
            return await GetConversationByIdAsync(
                conversation.ConversationId,
                buyerId,
                false);
        }
        //-------------------------------------------------------------------------
        //SEND message
        //Send a new message in an existing conversation
        //--------------------------------------------------------------------------
        public async Task<MessageResponseDto?> SendMessageAsync(
    int conversationId,
    int senderId,
    SendMessageRequestDto request,
    bool isSellerMode)
        {
            // Verify access using the active profile mode.
            var conversation = await GetConversationByIdAsync(
                conversationId,
                senderId,
                isSellerMode);

            if (conversation == null)
                return null;

            if (string.IsNullOrWhiteSpace(request.Content)
                || request.Content.Length > 2000)
            {
                return null;
            }

            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = request.Content.Trim(),
                MessageType = request.MessageType,
                IsRead = false,
                SentAt = DateTime.UtcNow,
            };

            await _context.Messages.AddAsync(message);

            var conversationEntity = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.ConversationId == conversationId
                    && c.DeletedAt == null);

            if (conversationEntity == null)
                return null;

            conversationEntity.LastMessageAt = message.SentAt;

            await _context.SaveChangesAsync();

            var savedMessage = await _context.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.MessageId == message.MessageId);

            return savedMessage != null
                ? MapToMessageDto(savedMessage)
                : null;
        }
        //--------------------------------------------------------------------------------------------
        //MARK messages as read
        //Marks all unread messages as read when a user opens a conversation
        //Only marks messages sent by OTHER participant as read
        //_____________________________________________________________________________________________
        public async Task MarkMessagesAsReadAsync(
    int conversationId,
    int userId,
    bool isSellerMode)
        {
            // Verify access before changing read status.
            var conversation = await GetConversationByIdAsync(
                conversationId,
                userId,
                isSellerMode);

            if (conversation == null)
                return;

            var unreadMessages = await _context.Messages
                .Where(m =>
                    m.ConversationId == conversationId
                    && m.SenderId != userId
                    && !m.IsRead)
                .ToListAsync();

            if (!unreadMessages.Any())
                return;

            foreach (var message in unreadMessages)
                message.IsRead = true;

            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS — MAP TO DTOs
        // ─────────────────────────────────────────────────────────────
        private static ConversationResponseDto MapToConversationDto(
            Conversation conversation, int currentUserId)
        {
            //Get the last message for the inbox preview
            var lastMessage=conversation.Messages?
                .OrderByDescending(m=>m.SentAt)
                .FirstOrDefault();

            //Count unread messages sent by the OTHER participants
            var unreadCount=conversation.Messages?
                .Count(m=>!m.IsRead && m.SenderId != currentUserId) ??0;

            return new ConversationResponseDto
            {
                ConversationId = conversation.ConversationId,
                ListingId = conversation.ListingId,
                ListingTitle = conversation.Listing?.Title ?? string.Empty,
                ListingPrice = conversation.Listing?.Price ?? 0,
                ListingCoverImage = conversation.Listing?.ListingImages?
                    .Where(i => i.IsCoverImage)
                    .Select(i => i.ImagePath)
                    .FirstOrDefault()
                    ?? conversation.Listing?.ListingImages?.FirstOrDefault()?.ImagePath,
                BuyerId = conversation.BuyerId,  
                BuyerFirstName=conversation.Buyer?.FirstName?? string.Empty,
                BuyerLastName= conversation.Buyer?.LastName?? string.Empty,

                BuyerUniversityName =
                    conversation.Buyer?.University?.Name ?? string.Empty,

                BuyerUniversityShortName =
                    conversation.Buyer?.University?.ShortName ?? string.Empty,

                BuyerStudentVerified =
                    conversation.Buyer?.StudentVerified ?? false,

                BuyerJoinedAt =
                    conversation.Buyer?.CreatedAt ?? DateTime.MinValue,

                SellerProfileId = conversation.SellerProfileId,
                StoreName = conversation.SellerProfile?.StoreName ?? string.Empty,

                SellerFirstName =
                    conversation.SellerProfile?.User?.FirstName ?? string.Empty,

                SellerLastName =
                    conversation.SellerProfile?.User?.LastName ?? string.Empty,

                SellerUniversityName =
                    conversation.SellerProfile?.User?.University?.Name ?? string.Empty,

                SellerUniversityShortName =
                    conversation.SellerProfile?.User?.University?.ShortName ?? string.Empty,

                SellerRating =
                    conversation.SellerProfile?.AverageRating ?? 0,

                SellerTotalSales =
                    conversation.SellerProfile?.TotalSales ?? 0,
                CreatedAt=conversation.CreatedAt,
                LastMessageAt= conversation.LastMessageAt,
                LastMessageContent= lastMessage?.Content,
                UnreadCount=unreadCount,
            };
        }
        private static MessageResponseDto MapToMessageDto(Message message)
        {
            return new MessageResponseDto
            {
                MessageId = message.MessageId,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderFirstName = message.Sender?.FirstName ?? string.Empty,
                SenderLastName = message.Sender?.LastName ?? string.Empty,
                Content = message.Content,
                MessageType = message.MessageType,
                IsRead = message.IsRead,
                SentAt = message.SentAt,
            };
        }
    }
}
