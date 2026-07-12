using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.Entities
{
   public class Location
    {
        public int LocationId { get; set; }
        public string Suburb { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Provide { get; set; } = string.Empty;
        public string? ResidentName { get; set; }

        public ICollection<User> Users { get; set; }=new List<User>();

    }
    
}
