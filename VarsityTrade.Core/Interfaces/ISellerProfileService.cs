using System;
using System.Collections.Generic;
using System.Text;
using VarsityTrade.Core.DTOs.SellerProfiles; // Provides SellerProfile DTOs


namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all seller profile operations
    public interface ISellerProfileService
    {
        //Get a seller profile by its ID-for the public profile page
        Task<SellerProfileResponseDto?> GetSellerProfileByIdAsync(int sellerProfileId);

        //Get a seller profile by the user ID-for the seller dashboard
        Task<SellerProfileResponseDto?> GetSellerProfileByUserIdAsync(int userId);

        //Activate a seller profile-called when a buyer wants to start selling
        Task<SellerProfileResponseDto?> ActivateSellerProfileAsync(int userId, SellerProfileRequestDto request);

        //Update an existing seller profile-called from seller profile settings
        Task<SellerProfileResponseDto?> UpdateSellerProfileAsync(int userID, SellerProfileRequestDto request);

    }
}
