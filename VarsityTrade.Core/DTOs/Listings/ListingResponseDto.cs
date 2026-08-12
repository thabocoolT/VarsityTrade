using System;
using System.Collections.Generic;
using System.Text;

namespace VarsityTrade.Core.DTOs.Listings
{
    // This DTO defines what the API returns when a listing is requested
    // It is a flattened, safe representation of the Listing entity
    // We never return entity classes directly — DTOs control exactly what the client sees
    public class ListingResponseDto
    {
        //Core listing identity
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description {  get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ListingType { get; set; }= string.Empty;
        public bool IsNegotiable { get; set; }
        public bool IsFeatured { get; set; }
        public int Quantity { get; set; }
        public int ViewCount {  get; set; }
        public bool CampusPickup { get; set; }
        public bool DeliveryAvailable { get; set; }
        public DateTime CreatedAt {  get; set; }
        public DateTime? UpdatedAt {  get; set; }
        public DateTime? ExpiresAt {  get; set; }

        //Status and condition as readable names - not just Ids
        //Returning the name means the client does not need extra lookups
        public string Status { get; set; }=string.Empty;
        public string Condition {  get; set; } = string.Empty;

        //Category info
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;


        //University info-confirms camous lock
        public int UniversityId { get; set; }
        public string UniversityName {  get; set; } = string.Empty;
        public string UniversityShortName {  get; set; } = string.Empty;

        //Seller info-enough for the buyer to see who is selling
        public int SellerProfileId { get; set; }
        public string StoreName {  get; set; } = string.Empty;
        public decimal SellerRating {  get; set; }

        //Cover image URL-the first image shown on listing cards
        public string? CoverImageUrl {  get; set; }
    }
}
