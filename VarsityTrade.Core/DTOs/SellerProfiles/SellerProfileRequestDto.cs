using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; // Provides Required validation attribute

namespace VarsityTrade.Core.DTOs.SellerProfiles
{
    // This DTO defines the data required to activate a seller profile
    // A buyer submits this when they want to start selling on the platform
    public class SellerProfileRequestDto
    {
        //Store Name is the public shop name buyers see on listings
        [Required(ErrorMessage = "Store name is required")]
        [MaxLength(150, ErrorMessage = "Store name cannot exceed 150 characters")]
        public string StoreName { get; set; } = string.Empty;

        //SellerBio is optional- a short description of the seller and what they sell
        [MaxLength(1000,ErrorMessage ="Bio cannot exceed 1000 characters")]
        public string? SellerBio { get; set; }

        //CampusPickup indicates whether the seller offers campus pickup
        public bool CampusPickup { get; set; } = true;

        //DeliveryAvailable indicates whether the seller can deliver to nearby residences
        public bool DeliveryAvailable {  get; set; }=false;

        //OpenToTrades indicates whether the seller accepts trade offers
        public bool OpenToTrades { get; set; } = true;
    }
}
