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
        public async Task<IEnumerable<ConversationResponseDto>> GetConversationsAsync(int userId)
        {
            //Find the seller profile for this user-may be null if not a seller
            var sellerProfile=await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            //Get the seller profile ID-0 if user has no seller profile
            //This allows the query to work for buyers who are not sellers
            var sellerProfileId = sellerProfile?.SellerProfileId ?? 0;

            //Load all conversations where the user is the buyer OR the seller
            //Include all related data needed for the inbox card
            var conversations=await _context.Conversations
                .Include(c => c.Listing)            //Load listing for title and price
                    .ThenInclude(l => l.ListingImages)     //Load listing images for thumbnail
                .Include(c => c.Buyer)              //Load buyer for name and profile picture
                .Include(c => c.SellerProfile)      //Load seller profile for name and profile picture
                .Include(c => c.Messages)            //Load messages for last message and timestamp
                .Where(c =>
                       (c.BuyerId==userId || c.SellerProfileId==sellerProfileId)
                       && c.DeletedAt==null)        //Exclude soft-deleted conversations
                .OrderByDescending(c => c.LastMessageAt ?? c.CreatedAt) //Order by most recent activity
                .ToListAsync();

            //Map each conversation to a ConversationResponseDto for the inbox view
            return conversations.Select(c=>MapToConversationDto(c, userId));



        }
        //----------------------------------------------------------------
        //GET Conversation by ID
        //Returns a single conversation by ID for the conversation page
        //----------------------------------------------------------------

        public async Task<ConversationResponseDto?> GetConversationByIdAsync(int conversationId, int userId)
        {
            //Find the seller profile for this user
            var sellerProfile=await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            var sellerProfileId=sellerProfile?.SellerProfileId ?? 0;

            //Load the conversation-must be participant to view it
            var conversation = await _context.Conversations
                .Include(c=> c.Listing)            //Load listing for title and price
                    .ThenInclude(l => l.ListingImages)     //Load listing images for thumbnail
                .Include(c => c.Buyer)              //Load buyer for name and profile picture
                .Include(c => c.SellerProfile)      //Load seller profile for name and profile picture
                .Include(c => c.Messages)            //Load messages for last message and timestamp
                .FirstOrDefaultAsync(c =>
                    c.ConversationId == conversationId &&
                    (c.BuyerId == userId || c.SellerProfileId == sellerProfileId) &&
                    c.DeletedAt == null); //Exclude soft-deleted conversations

            if (conversation == null)
                return null;

            return MapToConversationDto(conversation, userId);
        }
        //----------------------------------------------------------------
        //GET Messages in a Conversation
        //Returns all messages in a conversation-ordered oldest to newest
        //----------------------------------------------------------------
        public async Task<IEnumerable<MessageResponseDto>> GetMessagesAsync(int conversationId, int userId)
        {
            //Find the seller profile for this user
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            var sellerProfileId = sellerProfile?.SellerProfileId ?? 0;

            //Verify that the user is a participant in the conversation
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                    c.ConversationId == conversationId
                    && (c.BuyerId == userId || c.SellerProfileId == sellerProfileId)
                    && c.DeletedAt == null);

            //Return empty list if conversation not found or user is not a participant
            if (conversation == null)
                return Enumerable.Empty<MessageResponseDto>();

            //Load all messages in this conversation with sender info
            var message=await _context.Messages
                .Include(m=>m.Sender)       //Load sender for name and profile picture
                .Where(m=>
                    m.ConversationId==conversationId
                    && !m.DeletedBySender               //Exclude messages deleted by sender
                    && !m.DeletedByReceiver)            //Exclude messages deleted by receiver
                .OrderBy(m=>m.SentAt)               //Oldest first-chronological order
                .ToListAsync();

            //Mark messages as read when the user opens the conversation thread
            await MarkMessagesAsReadAsync(conversationId, userId);
            return message.Select(m=>MapToMessageDto(m));
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
                .Include(c => c.SellerProfile)
                .Include(c => c.Messages)
                .FirstOrDefaultAsync(c=>
                    c.BuyerId == buyerId
                    && c.ListingId == request.ListingId
                    && c.DeletedAt == null);

            //If conversation already exists just send the new message into it
            if(existing !=null)
            {
                var newMsg = new Message
                {
                    ConversationId = existing.ConversationId,
                    SenderId       =buyerId,
                    Content        =request.InitialMessage,
                    MessageType    ="Text",
                    IsRead         =false,
                    SentAt         =DateTime.UtcNow,
                };
                await _context.Messages.AddAsync(newMsg);

                //Update the LastMessageAt timestamp on the conversation
                existing.LastMessageAt= DateTime.UtcNow;
                _context.Conversations.Update(existing);
                await _context.SaveChangesAsync();

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

            //Create the first message in the conversation
            var initialMessage = new Message
            {
                ConversationId = conversation.ConversationId,
                SenderId = buyerId,
                Content = request.InitialMessage,
                MessageType="Text",
                IsRead=false,
                SentAt=DateTime.UtcNow,
            };
            await _context.Messages.AddAsync(initialMessage);
            await _context.SaveChangesAsync();

            // Reload with all related data for the response
            return await GetConversationByIdAsync(conversation.ConversationId, buyerId);
        }
        //-------------------------------------------------------------------------
        //SEND message
        //Send a new message in an existing conversation
        //--------------------------------------------------------------------------
        public async Task<MessageResponseDto?> SendMessageAsync(int conversationId, int senderId, SendMessageRequestDto request)
        {
            // Find the sender's seller profile — may be null if they are a buyer
            var sellerProfile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == senderId && sp.IsActive);

            var sellerProfileId = sellerProfile?.SellerProfileId ?? 0;

            //Verify then sender is a participant in this conversation
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c =>
                c.ConversationId == conversationId
                && (c.BuyerId==senderId || c.SellerProfileId== sellerProfileId)
                && c.DeletedAt==null);

            //Return null if conversation not found or sender is not a participant
            if (conversation == null)
                return null;


            //Create the new message
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = request.Content,
                MessageType = request.MessageType,
                IsRead = false,            // Unread until recipient opens the thread
                SentAt = DateTime.UtcNow,
            };
            await _context.Messages.AddAsync(message);

            //Update the LastMessageAt on the conversation on the conversation for inbox sorting
            conversation.LastMessageAt= DateTime.UtcNow;
            _context.Conversations.Update(conversation);

            await _context.SaveChangesAsync();

            //Reload the message with sender info for the response DTO
            var savedMessage=await _context.Messages
                .Include(m=>m.Sender)
                .FirstOrDefaultAsync(m=>m.MessageId==message.MessageId);

            return savedMessage != null ? MapToMessageDto(savedMessage) : null;
        }
        //--------------------------------------------------------------------------------------------
        //MARK messages as read
        //Marks all unread messages as read when a user opens a conversation
        //Only marks messages sent by OTHER participant as read
        //_____________________________________________________________________________________________
        public async Task MarkMessagesAsReadAsync(int conversationId, int userId)
        {
            //Get all unread messages in this conversation NOT sent by this user
            //We only mark messages as read that weresent TO this user
            var unreadMessages=await _context.Messages
                .Where(m=>
                   m.ConversationId == conversationId
                    && m.SenderId != userId  // Only mark messages from the other person
                    && !m.IsRead)            // Only mark currently unread messages
                .ToListAsync();

            if (!unreadMessages.Any())
                return; //Nothing to mark as read

            //Mark each message as read
            foreach(var msg in unreadMessages)
                msg.IsRead = true;
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
                SellerProfileId= conversation.SellerProfileId,
                StoreName= conversation.SellerProfile?.StoreName ?? string.Empty,
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
