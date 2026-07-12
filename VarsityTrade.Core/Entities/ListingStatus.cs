using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
    public class ListingStatus
    {
        public int ListingStatusId { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Listing> Listings { get; set; } = new List<Listing>();
    }
}
