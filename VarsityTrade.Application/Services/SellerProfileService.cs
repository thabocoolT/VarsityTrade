using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore; // Provides FirstOrDefaultAsync
using VarsityTrade.Core.DTOs.SellerProfiles; // Provides SellerProfile DTOs
using VarsityTrade.Core.Entities; // Provides SellerProfile entity
using VarsityTrade.Core.Interfaces; // Provides ISellerProfileService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.Application.Services
{
    // SellerProfileService handles all business logic for seller profile operations
    public class SellerProfileService : ISellerProfileService
    {
        // DbContext injected for database access
        private readonly VarsityTradeDbContext _context;

        public SellerProfileService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────────────────────
        // GET BY ID
        // ─────────────────────────────────────────────────────────────
        public async Task<SellerProfileResponseDto?> GetSellerProfileByIdAsync(int sellerProfileId)
        {
            // Load the seller profile with related user and university data
            var profile = await _context.SellerProfiles
                .Include(sp => sp.User)                  // Load user for name display
                    .ThenInclude(u => u.University)      // Load university for campus display
                .FirstOrDefaultAsync(sp =>
                    sp.SellerProfileId == sellerProfileId
                    && sp.IsActive); // Only return active profiles

            if (profile == null)
                return null;

            return MapToResponseDto(profile);
        }

        // ─────────────────────────────────────────────────────────────
        // GET BY USER ID
        // Used by the seller dashboard to load the seller's own profile
        // ─────────────────────────────────────────────────────────────
        public async Task<SellerProfileResponseDto?> GetSellerProfileByUserIdAsync(int userId)
        {
            var profile = await _context.SellerProfiles
                .Include(sp => sp.User)
                    .ThenInclude(u => u.University)
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            if (profile == null)
                return null;

            return MapToResponseDto(profile);
        }

        // ─────────────────────────────────────────────────────────────
        // ACTIVATE SELLER PROFILE
        // Called when a buyer activates their seller profile for the first time
        // Each user can only have one seller profile — enforced by unique constraint
        // ─────────────────────────────────────────────────────────────
        public async Task<SellerProfileResponseDto?> ActivateSellerProfileAsync(
     int userId,
     SellerProfileRequestDto request)
        {
            // Check whether this user already has a seller profile
            var existing = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId);

            // Profile already exists
            if (existing != null)
            {
                // If it was previously inactive, reactivate it
                existing.StoreName = request.StoreName;
                existing.SellerBio = request.SellerBio;
                existing.CampusPickup = request.CampusPickup;
                existing.DeliveryAvailable = request.DeliveryAvailable;
                existing.OpenToTrades = request.OpenToTrades;
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;

                _context.SellerProfiles.Update(existing);
                await _context.SaveChangesAsync();

                return await GetSellerProfileByUserIdAsync(userId);
            }

            // No existing profile — create one
            var profile = new SellerProfile
            {
                UserId = userId,
                StoreName = request.StoreName,
                SellerBio = request.SellerBio,
                CampusPickup = request.CampusPickup,
                DeliveryAvailable = request.DeliveryAvailable,
                OpenToTrades = request.OpenToTrades,
                AverageRating = 0.00m,
                TotalSales = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.SellerProfiles.AddAsync(profile);
            await _context.SaveChangesAsync();

            return await GetSellerProfileByUserIdAsync(userId);
        }

        // ─────────────────────────────────────────────────────────────
        // UPDATE SELLER PROFILE
        // Called from seller profile settings page
        // ─────────────────────────────────────────────────────────────
        public async Task<SellerProfileResponseDto?> UpdateSellerProfileAsync(int userId, SellerProfileRequestDto request)
        {
            // Find the seller profile belonging to this user
            var profile = await _context.SellerProfiles
                .FirstOrDefaultAsync(sp => sp.UserId == userId && sp.IsActive);

            if (profile == null)
                return null; // Profile not found — cannot update

            // Update only the fields the seller is allowed to change
            profile.StoreName = request.StoreName;
            profile.SellerBio = request.SellerBio;
            profile.CampusPickup = request.CampusPickup;
            profile.DeliveryAvailable = request.DeliveryAvailable;
            profile.OpenToTrades = request.OpenToTrades;
            profile.UpdatedAt = DateTime.UtcNow;

            _context.SellerProfiles.Update(profile);
            await _context.SaveChangesAsync();

            return await GetSellerProfileByUserIdAsync(userId);
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPER — MAP TO RESPONSE DTO
        // ─────────────────────────────────────────────────────────────
        private static SellerProfileResponseDto MapToResponseDto(SellerProfile profile)
        {
            return new SellerProfileResponseDto
            {
                SellerProfileId = profile.SellerProfileId,
                UserId = profile.UserId,
                StoreName = profile.StoreName,
                SellerBio = profile.SellerBio,
                CampusPickup = profile.CampusPickup,
                DeliveryAvailable = profile.DeliveryAvailable,
                OpenToTrades = profile.OpenToTrades,
                AverageRating = profile.AverageRating,
                TotalSales = profile.TotalSales,
                IsActive = profile.IsActive,
                CreatedAt = profile.CreatedAt,

                // Map user and university info for public profile display
                FirstName = profile.User?.FirstName ?? string.Empty,
                LastName = profile.User?.LastName ?? string.Empty,
                UniversityName = profile.User?.University?.Name ?? string.Empty,
                UniversityShortName = profile.User?.University?.ShortName ?? string.Empty,
            };
        }
    }
}