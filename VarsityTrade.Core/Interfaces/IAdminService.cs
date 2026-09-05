using VarsityTrade.Core.DTOs.Admin; // Provides Admin DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all admin operations
    // All methods require admin-level access enforced at the controller level
    public interface IAdminService
    {
        // ── USER MANAGEMENT ──────────────────────────────────────────
        Task<IEnumerable<AdminUserResponseDto>> GetAllUsersAsync();
        Task<AdminUserResponseDto?> GetUserByIdAsync(int userId);
        Task<AdminUserResponseDto?> UpdateUserAsync(int userId, AdminUpdateUserDto request);

        // ── LISTING MANAGEMENT ────────────────────────────────────────
        Task<IEnumerable<AdminListingResponseDto>> GetAllListingsAsync();
        Task<bool> RemoveListingAsync(int listingId, int adminUserId);
        Task<bool> ToggleFeaturedAsync(int listingId);

        // ── REPORTS ───────────────────────────────────────────────────
        Task<IEnumerable<ReportResponseDto>> GetAllReportsAsync();
        Task<ReportResponseDto?> GetReportByIdAsync(int reportId);
        Task<bool> ResolveReportAsync(int reportId, int adminUserId, ResolveReportDto request);

        // ── HERO BANNER ───────────────────────────────────────────────
        Task<IEnumerable<HeroBannerSlideResponseDto>> GetAllSlidesAsync();
        Task<HeroBannerSlideResponseDto?> CreateSlideAsync(HeroBannerSlideRequestDto request);
        Task<HeroBannerSlideResponseDto?> UpdateSlideAsync(int slideId, HeroBannerSlideRequestDto request);
        Task<bool> DeleteSlideAsync(int slideId);
        Task<bool> ToggleSlideVisibilityAsync(int slideId);

        // ── SYSTEM SETTINGS ───────────────────────────────────────────
        Task<IEnumerable<VarsityTrade.Core.Entities.SystemSettings>> GetSystemSettingsAsync();
        Task<bool> UpdateSystemSettingAsync(string key, string value);

        // ── PLATFORM STATS ────────────────────────────────────────────
        Task<PlatformStatsResponseDto> GetPlatformStatsAsync();
    }
}