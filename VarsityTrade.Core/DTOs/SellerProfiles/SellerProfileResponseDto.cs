using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.DTOs.SellerProfiles
{
    // This DTO defines what the API returns when a seller profile is requested
    // Used on the public seller profile page and on listing cards
    public class SellerProfileResponseDto
    {
        public int SellerProfileId { get; set; }
        public int UserId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string? SellerBio {  get; set; }
        public bool CampusPickup { get; set; }
        public bool DeliveryAvailable { get; set; }
        public bool OpenToTrades { get; set; }

        //AverageRating and totalsales are cashed fields updated after each transaction
        //Returnin them here avoids expensive aggregate queries on every request
        public decimal AverageRating {  get; set; }
        public int TotalSales { get; set; }
        public bool IsActive {  get; set; }
        public DateTime CreatedAt {  get; set; }

        //Seller's name and university-shown on the public profile page
        public string FirstName { get; set; }= string.Empty;
        public string LastName { get; set; }=string.Empty;
        public string UniversityName {  get; set; } = string.Empty;
        public string UniversityShortName {  get; set; } = string.Empty;

    }
}
