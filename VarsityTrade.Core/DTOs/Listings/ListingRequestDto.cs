using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations; // Provides Required, Range validation attributes

namespace VarsityTrade.Core.DTOs.Listings
{
    //This DTO defines the data a seller must provide when create or editing a listing
    //The client sends this in the request body to  the create and update endpoints
    public class ListingRequestDto
    {
        //Title is request-every listing must have a clear title
        [Required(ErrorMessage ="Title is required")]
        [MaxLength(200, ErrorMessage ="Title cannot exceed 200 characters")]
        public string Title { get; set; }=string.Empty;

        //Description is required-buyers need details about the item
        [Required(ErrorMessage ="Description is required")]
        public string Description {  get; set; }=string.Empty;

        //Price must be zero or greater-free items are allowed
        [Required(ErrorMessage ="Price is required")]
        [Range(0, double.MaxValue, ErrorMessage ="Price must be zero or greater")]
        public decimal Price { get; set; }


        //CategoryId links the listing to a category for filtering
        [Required(ErrorMessage ="Category is required")]
        public int CategoryId { get; set; }

        //ConditionId describes the item condition-New, Like New, fair
        [Required(ErrorMessage ="Condition is required")]
        public int ConditionId { get; set; }

        //ListingType defines whether it is for Sale, Trade, or both
        [Required(ErrorMessage = "Listing type is required")]
        public string ListingType { get; set; } = "Sale";

        //IsNegotiable indicates whether the seller accepts price negotiation
        public bool IsNegotiable { get; set; } = false;

        //Quantity defaults to 1-seller can specify more multiple items
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        //CampusPickup indicates whether the seller offers campus pickup
        public bool CampusPickup { get; set; } = true;


        //DeliveryAvailable indicates whether the seller can deliver
        public bool DeliveryAvailable { get; set; } = false;

            
    }
}
