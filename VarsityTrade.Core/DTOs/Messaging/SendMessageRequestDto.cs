using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; // Provides Required and MaxLength validation

namespace VarsityTrade.Core.DTOs.Messaging
{
    //This DTO defines the data required to send a message in a conversation
    //Used by both buyers and sellers when replying in a conversion
    public class SendMessageRequestDto
    {
        //The  conversation content-required, cannot send an empty message
        [Required(ErrorMessage = "Message content is required.")]
        [MaxLength(2000, ErrorMessage = "Message content cannot exceed 2000 characters.")]
        public string Content { get; set; } = string.Empty;

        //MessageType defaults to "Text" but can also be "Image" or "System"
        public string MessageType { get; set; } = "Text";
    }
    
}
