using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class SavedListing
    {
        public int SavedListingId { get; set; }
        public int UserId { get; set; }
        public int ListingId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;


        public User User { get; set; } = null!;
        public Listing Listing { get; set; } = null!;


    }
    
}
