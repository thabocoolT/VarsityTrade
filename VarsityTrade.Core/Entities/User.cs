using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;


namespace VarsityTrade.Core.Entities
{
    public class User : IdentityUser<int>
    {
        public int UniversityId { get; set; }
        public int? LocationId { get; set; }
        public string FistName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set;  }
        public string? Bio {  get; set; }
        public string? StudentNumber { get; set; }
        public bool StudentVerified { get; set; }
        public string Role { get; set; } = "Buyer";
        public bool IsActive { get; set; } = true;
        public bool IsBanned { get; set; }=false;

        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt {  get; set; }
        public DateTime? DeletedAt { get; set; }

        public University University { get; set; } = null!;
        public Location? Location { get; set; }
        public SellerProfile? SellerProfile { get; set; }
        public ICollection<SavedListing> SavedListings { get; set; } = new List<SavedListing>();
        public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public ICollection<Offer> Offers {  get; set; }=new List<Offer>();
        public ICollection<Transaction> Transactions{  get; set; } = new List<Transaction>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Report> SubmittedReports { get; set; } = new List<Report>();
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    
    }
    
    
}
