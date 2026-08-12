using System;
using System.Collections.Generic;
using System.Text;
using VarsityTrade.Core.DTOs.Messaging; // Provides Messaging DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all messaging operations
    // Keeps the controller decoupled from the concrete implementation
    public interface IMessagingService
    {
        //Get all conversations for a user-shown on the inbox page
        //Returns conversations where the user is either the buyer or seller
        Task<IEnumerable<ConversationResponseDto>> GetConversationsAsync(int userId);

        //Get a single messages in a conversation by ID-for the conversation page
        Task<IEnumerable<ConversationResponseDto?>> GetConversationByIdAsync(int conversationId, int userId);

        //Get all messages in a conversation-ordered oldest to newest
        Task<IEnumerable<MessageResponseDto>> GetMessagesAsync(int conversationId, int userId);

        //Start a new conversation - called when a buyer cliscks message seller on a listing page
        //Returns the existing conversation if one already exists for this buyer and listing
        Task<ConversationResponseDto?> StartConversationAsync(int buyerId, StartConversationRequestDto request);


        //Send a message in an existing conversation
        Task<MessageResponseDto?> SendMessageAsync(int senderId, int conversationId, SendMessageRequestDto request);

        //Mark all unread messages in a conversation as read
        //Called when a user opens a conversation thread
        Task MarkMessagesAsReadAsync(int conversationId, int userId);

    }
}
