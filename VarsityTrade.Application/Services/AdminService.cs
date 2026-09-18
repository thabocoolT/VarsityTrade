using Microsoft.EntityFrameworkCore; // Provides Include, FirstOrDefaultAsync, ToListAsync, CountAsync
using VarsityTrade.Core.DTOs.Admin; // Provides Admin DTOs
using VarsityTrade.Core.Entities; // Provides all entities
using VarsityTrade.Core.Interfaces; // Provides IAdminService
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext
using SystemSettings = VarsityTrade.Core.Entities.SystemSettings; // Alias to avoid conflict

namespace VarsityTrade.Application.Services
{
    // AdminService handles all admin business logic
    // All operations here are cross-university — admins see everything
    public class AdminService : IAdminService
    {
        // DbContext injected for all database operations
        private readonly VarsityTradeDbContext _context;

        public AdminService(VarsityTradeDbContext context)
        {
            _context = context;
        }

        // ══════════════════════════════════════════════════════════════
        // USER MANAGEMENT
        // ══════════════════════════════════════════════════════════════

        public async Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync()
        {
            // Load all users with university, location, and seller profile
            var users = await _context.Users
                .Include(u => u.University)      // For university name display
                .Include(u => u.Location)        // For suburb and res display
                .Include(u => u.SellerProfile)   // To check if seller profile exists
                .Where(u => u.DeletedAt == null) // Exclude soft-deleted users
                .OrderByDescending(u => u.CreatedAt) // Newest users first
                .ToListAsync();

            return users.Select(u => MapUserToDto(u));
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAuditLogsAsync(
            int count = 10)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.Created)
                .Take(count)
                .ToListAsync();
        }


        public async Task<AdminUserResponseDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.University)
                .Include(u => u.Location)
                .Include(u => u.SellerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

            return user == null ? null : MapUserToDto(user);
        }

        public async Task<AdminUserResponseDto?> UpdateUserAsync(int userId, AdminUpdateUserDto request)
        {
            // Find the user to update
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null);

            if (user == null)
                return null;

            // Update only the admin-controlled fields
            user.IsActive = request.IsActive;
            user.IsBanned = request.IsBanned;
            user.StudentVerified = request.StudentVerified;
            user.UpdateAt = DateTime.UtcNow;

            // If banned or deactivated also deactivate their seller profile
            if (!request.IsActive || request.IsBanned)
            {
                var sellerProfile = await _context.SellerProfiles
                    .FirstOrDefaultAsync(sp => sp.UserId == userId);

                if (sellerProfile != null)
                {
                    // Pause the seller profile when user is banned or deactivated
                    sellerProfile.IsActive = false;
                    sellerProfile.UpdatedAt = DateTime.UtcNow;
                    _context.SellerProfiles.Update(sellerProfile);
                }
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return await GetUserByIdAsync(userId);
        }

        // ══════════════════════════════════════════════════════════════
        // LISTING MANAGEMENT
        // ══════════════════════════════════════════════════════════════

        public async Task<IEnumerable<AdminListingResponseDto>> GetAllListingsAsync()
        {
            // Load all listings across all universities — admin has cross-campus view
            var listings = await _context.Listings
                .Include(l => l.University)
                .Include(l => l.Category)
                .Include(l => l.Condition)
                .Include(l => l.ListingStatus)
                .Include(l => l.SellerProfile)
                    .ThenInclude(sp => sp.User)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            return listings.Select(l => MapListingToDto(l));
        }

        public async Task<bool> RemoveListingAsync(int listingId, int adminUserId)
        {
            // Find the listing — admin can remove any listing
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.ListingId == listingId && l.DeletedAt == null);

            if (listing == null)
                return false;

            // Get the Deleted status
            var deletedStatus = await _context.ListingStatuses
                .FirstOrDefaultAsync(ls => ls.Name == "Deleted");

            // Soft delete the listing
            listing.DeletedAt = DateTime.UtcNow;
            listing.UpdatedAt = DateTime.UtcNow;

            if (deletedStatus != null)
                listing.ListingStatusId = deletedStatus.ListingStatusId;

            _context.Listings.Update(listing);

            // Log the admin action in the audit log
            await LogAuditAsync(adminUserId, "RemoveListing", "Listing", listingId);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleFeaturedAsync(int listingId)
        {
            // Find the listing
            var listing = await _context.Listings
                .FirstOrDefaultAsync(l => l.ListingId == listingId && l.DeletedAt == null);

            if (listing == null)
                return false;

            // Toggle the IsFeatured flag
            listing.IsFeatured = !listing.IsFeatured;
            listing.UpdatedAt = DateTime.UtcNow;

            _context.Listings.Update(listing);
            await _context.SaveChangesAsync();
            return true;
        }

        // ══════════════════════════════════════════════════════════════
        // REPORTS
        // ══════════════════════════════════════════════════════════════

        public async Task<IEnumerable<ReportResponseDto>> GetAllReportsAsync()
        {
            // Load all reports with related data for the reports queue
            var reports = await _context.Reports
                .Include(r => r.ReportedBy)      // Who submitted the report
                .Include(r => r.Listing)         // The listing being reported
                .Include(r => r.ReportedUser)    // The user being reported
                .Include(r => r.ResolvedBy)      // Who resolved the report
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reports.Select(r => MapReportToDto(r));
        }

        public async Task<ReportResponseDto?> GetReportByIdAsync(int reportId)
        {
            var report = await _context.Reports
                .Include(r => r.ReportedBy)
                .Include(r => r.Listing)
                .Include(r => r.ReportedUser)
                .Include(r => r.ResolvedBy)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            return report == null ? null : MapReportToDto(report);
        }

        public async Task<bool> ResolveReportAsync(int reportId, int adminUserId, ResolveReportDto request)
        {
            // Find the report
            var report = await _context.Reports
                .FirstOrDefaultAsync(r => r.ReportId == reportId);

            if (report == null)
                return false;

            // Update the report with the resolution details
            report.Status = request.Status;
            report.AdminNotes = request.AdminNotes;
            report.ResolvedById = adminUserId;
            report.ResolvedAt = DateTime.UtcNow;
            report.UpdatedAt = DateTime.UtcNow;

            _context.Reports.Update(report);

            // Log the admin action
            await LogAuditAsync(adminUserId, "ResolveReport", "Report", reportId);

            await _context.SaveChangesAsync();
            return true;
        }

        // ══════════════════════════════════════════════════════════════
        // HERO BANNER
        // ══════════════════════════════════════════════════════════════

        public async Task<IEnumerable<HeroBannerSlideResponseDto>> GetAllSlidesAsync()
        {
            // Load all slides ordered by sort order
            var slides = await _context.HeroBannerSlides
                .Include(s => s.Listing) // Load listing for title display
                .OrderBy(s => s.SortOrder)
                .ToListAsync();

            return slides.Select(s => MapSlideToDto(s));
        }

        public async Task<HeroBannerSlideResponseDto?> CreateSlideAsync(HeroBannerSlideRequestDto request)
        {
            var slide = new HeroBannerSlide
            {
                SlideType = request.SlideType,
                ListingId = request.ListingId,
                ReviewId = request.ReviewId,
                Title = request.Title,
                Subtitle = request.Subtitle,
                ImageUrl = request.ImageUrl,
                ButtonText = request.ButtonText,
                ButtonUrl = request.ButtonUrl,
                IsVisible = request.IsVisible,
                SortOrder = request.SortOrder,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow,
            };

            await _context.HeroBannerSlides.AddAsync(slide);
            await _context.SaveChangesAsync();

            // Reload with related data for the response
            var saved = await _context.HeroBannerSlides
                .Include(s => s.Listing)
                .FirstOrDefaultAsync(s => s.HeroBannerSlideId == slide.HeroBannerSlideId);

            return saved == null ? null : MapSlideToDto(saved);
        }

        public async Task<HeroBannerSlideResponseDto?> UpdateSlideAsync(int slideId, HeroBannerSlideRequestDto request)
        {
            var slide = await _context.HeroBannerSlides
                .FirstOrDefaultAsync(s => s.HeroBannerSlideId == slideId);

            if (slide == null)
                return null;

            // Update all editable fields
            slide.SlideType = request.SlideType;
            slide.ListingId = request.ListingId;
            slide.ReviewId = request.ReviewId;
            slide.Title = request.Title;
            slide.Subtitle = request.Subtitle;
            slide.ImageUrl = request.ImageUrl;
            slide.ButtonText = request.ButtonText;
            slide.ButtonUrl = request.ButtonUrl;
            slide.IsVisible = request.IsVisible;
            slide.SortOrder = request.SortOrder;
            slide.StartDate = request.StartDate;
            slide.EndDate = request.EndDate;
            slide.UpdatedAt = DateTime.UtcNow;

            _context.HeroBannerSlides.Update(slide);
            await _context.SaveChangesAsync();

            var updated = await _context.HeroBannerSlides
                .Include(s => s.Listing)
                .FirstOrDefaultAsync(s => s.HeroBannerSlideId == slideId);

            return updated == null ? null : MapSlideToDto(updated);
        }

        public async Task<bool> DeleteSlideAsync(int slideId)
        {
            var slide = await _context.HeroBannerSlides
                .FirstOrDefaultAsync(s => s.HeroBannerSlideId == slideId);

            if (slide == null)
                return false;

            // Hard delete — banner slides do not need soft delete
            _context.HeroBannerSlides.Remove(slide);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleSlideVisibilityAsync(int slideId)
        {
            var slide = await _context.HeroBannerSlides
                .FirstOrDefaultAsync(s => s.HeroBannerSlideId == slideId);

            if (slide == null)
                return false;

            // Toggle visibility — hide visible slides, show hidden slides
            slide.IsVisible = !slide.IsVisible;
            slide.UpdatedAt = DateTime.UtcNow;

            _context.HeroBannerSlides.Update(slide);
            await _context.SaveChangesAsync();
            return true;
        }

        // ══════════════════════════════════════════════════════════════
        // SYSTEM SETTINGS
        // ══════════════════════════════════════════════════════════════

        public async Task<IEnumerable<SystemSettings>> GetSystemSettingsAsync()
        {
            // Return all system settings ordered by key name
            return await _context.SystemSettings
                .OrderBy(s => s.Key)
                .ToListAsync();
        }

        public async Task<bool> UpdateSystemSettingAsync(string key, string value)
        {
            // Find the setting by key
            var setting = await _context.SystemSettings
                .FirstOrDefaultAsync(s => s.Key == key);

            if (setting == null)
                return false;

            // Update the value
            setting.Value = value;

            _context.SystemSettings.Update(setting);
            await _context.SaveChangesAsync();
            return true;
        }

        // ══════════════════════════════════════════════════════════════
        // PLATFORM STATS
        // ══════════════════════════════════════════════════════════════

        public async Task<PlatformStatsResponseDto> GetPlatformStatsAsync()
        {
            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);

            var totalUsers =
                await _context.Users.CountAsync(
                    u => u.DeletedAt == null);

            var activeUsers =
                await _context.Users.CountAsync(
                    u => u.IsActive
                      && !u.IsBanned
                      && u.DeletedAt == null);

            var verifiedUsers =
                await _context.Users.CountAsync(
                    u => u.IsActive
                      && !u.IsBanned
                      && u.StudentVerified
                      && u.DeletedAt == null);

            var verificationRate =
                activeUsers == 0
                    ? 0
                    : (int)Math.Round(
                        verifiedUsers * 100m / activeUsers);

            var bannedUsers =
                await _context.Users.CountAsync(
                    u => u.IsBanned
                      && u.DeletedAt == null);

            var deactivatedUsers =
                await _context.Users.CountAsync(
                    u => !u.IsActive
                      && !u.IsBanned
                      && u.DeletedAt == null);

            var newUsersThisWeek =
                await _context.Users.CountAsync(
                    u => u.CreatedAt >= oneWeekAgo
                      && u.DeletedAt == null);

            var totalListings =
                await _context.Listings.CountAsync();

            var activeListings =
                await _context.Listings.CountAsync(
                    l => l.DeletedAt == null
                      && l.ListingStatus.Name == "Active");

            var soldListings =
                await _context.Listings.CountAsync(
                    l => l.ListingStatus.Name == "Sold");

            var deletedListings =
                await _context.Listings.CountAsync(
                    l => l.DeletedAt != null);

            var totalTransactions =
                await _context.Transactions.CountAsync();

            var completedTransactions =
                await _context.Transactions.CountAsync(
                    t => t.Status == "Completed");

            var totalMessages =
                await _context.Messages.CountAsync();

            var totalConversations =
                await _context.Conversations.CountAsync(
                    c => c.DeletedAt == null);

            var openReports =
                await _context.Reports.CountAsync(
                    r => r.Status == "Open"
                      || r.Status == "UnderReview");

            var resolvedReports =
                await _context.Reports.CountAsync(
                    r => r.Status == "Resolved"
                      || r.Status == "Dismissed");

            var totalReports =
                await _context.Reports.CountAsync();

            var totalSellers =
                await _context.SellerProfiles.CountAsync();

            var activeSellers =
                await _context.SellerProfiles.CountAsync(
                    sp => sp.IsActive);

            return new PlatformStatsResponseDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,

                VerifiedUsers = verifiedUsers,
                VerificationRate = verificationRate,

                BannedUsers = bannedUsers,
                DeactivatedUsers = deactivatedUsers,
                NewUsersThisWeek = newUsersThisWeek,

                TotalListings = totalListings,
                ActiveListings = activeListings,
                SoldListings = soldListings,
                DeletedListings = deletedListings,

                TotalTransactions = totalTransactions,
                CompletedTransactions = completedTransactions,

                TotalMessages = totalMessages,
                TotalConversations = totalConversations,

                OpenReports = openReports,
                ResolvedReports = resolvedReports,
                TotalReports = totalReports,

                TotalSellerProfiles = totalSellers,
                ActiveSellerProfiles = activeSellers
            };
        }



        // ══════════════════════════════════════════════════════════════
        // PRIVATE HELPERS
        // ══════════════════════════════════════════════════════════════

        // Log admin actions to the AuditLog table for accountability
        private async Task LogAuditAsync(int userId, string action, string entity, int entityId)
        {
            var log = new AuditLog
            {
                UserId = userId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Created = DateTime.UtcNow,
            };
            await _context.AuditLogs.AddAsync(log);
        }

        private static AdminUserResponseDto MapUserToDto(User user)
        {
            return new AdminUserResponseDto
            {
                UserId = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Role = user.Role,
                IsActive = user.IsActive,
                IsBanned = user.IsBanned,
                StudentVerified = user.StudentVerified,
                StudentNumber = user.StudentNumber,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                UniversityId = user.UniversityId,
                UniversityName = user.University?.Name ?? string.Empty,
                UniversityShortName = user.University?.ShortName ?? string.Empty,
                Suburb = user.Location?.Suburb,
                ResidenceName = user.Location?.ResidenceName,
                HasSellerProfile = user.SellerProfile != null,
                StoreName = user.SellerProfile?.StoreName,
            };
        }

        private static AdminListingResponseDto MapListingToDto(Listing listing)
        {
            return new AdminListingResponseDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Price = listing.Price,
                Status = listing.ListingStatus?.Name ?? string.Empty,
                Condition = listing.Condition?.Name ?? string.Empty,
                CategoryName = listing.Category?.Name ?? string.Empty,
                ViewCount = listing.ViewCount,
                IsFeatured = listing.IsFeatured,
                CreatedAt = listing.CreatedAt,
                DeletedAt = listing.DeletedAt,
                UniversityName = listing.University?.Name ?? string.Empty,
                UniversityShortName = listing.University?.ShortName ?? string.Empty,
                SellerProfileId = listing.SellerProfileId,
                StoreName = listing.SellerProfile?.StoreName ?? string.Empty,
                SellerFirstName = listing.SellerProfile?.User?.FirstName ?? string.Empty,
                SellerLastName = listing.SellerProfile?.User?.LastName ?? string.Empty,
            };
        }

        private static ReportResponseDto MapReportToDto(Report report)
        {
            return new ReportResponseDto
            {
                ReportId = report.ReportId,
                ReportType = report.ReportType,
                Description = report.Description,
                EvidenceImage = report.EvidenceImage,
                Status = report.Status,
                AdminNotes = report.AdminNotes,
                CreatedAt = report.CreatedAt,
                ResolvedAt = report.ResolvedAt,
                ReportedById = report.ReportedById,
                ReportedByFirstName = report.ReportedBy?.FirstName ?? string.Empty,
                ReportedByLastName = report.ReportedBy?.LastName ?? string.Empty,
                ListingId = report.ListingId,
                ListingTitle = report.Listing?.Title,
                ReportedUserId = report.ReportedUserId,
                ReportedUserFirstName = report.ReportedUser?.FirstName,
                ReportedUserLastName = report.ReportedUser?.LastName,
                ResolvedById = report.ResolvedById,
                ResolvedByFirstName = report.ResolvedBy?.FirstName,
            };
        }

        private static HeroBannerSlideResponseDto MapSlideToDto(HeroBannerSlide slide)
        {
            return new HeroBannerSlideResponseDto
            {
                HeroBannerSlideId = slide.HeroBannerSlideId,
                SlideType = slide.SlideType,
                ListingId = slide.ListingId,
                ReviewId = slide.ReviewId,
                Title = slide.Title,
                Subtitle = slide.Subtitle,
                ImageUrl = slide.ImageUrl,
                ButtonText = slide.ButtonText,
                ButtonUrl = slide.ButtonUrl,
                IsVisible = slide.IsVisible,
                SortOrder = slide.SortOrder,
                StartDate = slide.StartDate,
                EndDate = slide.EndDate,
                CreatedAt = slide.CreatedAt,
                UpdatedAt = slide.UpdatedAt,
                ListingTitle = slide.Listing?.Title,
            };
        }
    }
}