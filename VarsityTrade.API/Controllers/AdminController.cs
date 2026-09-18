using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using System.Security.Claims; // Provides ClaimTypes for reading JWT claims
using VarsityTrade.Core.DTOs.Admin; // Provides Admin DTOs
using VarsityTrade.Core.Interfaces; // Provides IAdminService

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // All routes start with /api/admin
    [Authorize] // All admin endpoints require authentication
    public class AdminController : ControllerBase
    {
        // IAdminService injected — controller stays thin
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // Helper to read user ID from JWT token
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        // Helper to check if the current user has the Admin role
        // Reads the role claim from the JWT token
        // Helper to check if the current user has the Admin role
        // Checks all possible claim locations to handle different JWT configurations
        private bool IsAdmin()
        {
            // Print all claims to help diagnose — remove after fixing
            var allClaims = User.Claims.Select(c => $"{c.Type}={c.Value}");

            // Check custom role claim first
            var roleClaim = User.FindFirst("role");
            if (roleClaim?.Value == "Admin")
                return true;

            // Check standard ClaimTypes.Role as fallback
            var standardRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role);
            if (standardRole?.Value == "Admin")
                return true;

            // Check the http claim type as another fallback
            var httpRole = User.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            if (httpRole?.Value == "Admin")
                return true;

            return false;
        }

        // ── USER MANAGEMENT ──────────────────────────────────────────

        // GET /api/admin/users
        /// <summary>Returns all registered users across all universities — Admin only.</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!IsAdmin())
                return Forbid(); // 403 — only admins can access this

            var users = await _adminService.GetAllUsersAsync();
            return Ok(users);
        }

        // GET /api/admin/users/{id}
        /// <summary>Returns a single user by ID — Admin only.</summary>
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            if (!IsAdmin())
                return Forbid();

            var user = await _adminService.GetUserByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(user);
        }

        // PUT /api/admin/users/{id}
        /// <summary>Updates a user's details — Admin only.</summary>
        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] AdminUpdateUserDto request)
        {
            if (!IsAdmin())
                return Forbid();

            var result = await _adminService.UpdateUserAsync(id, request);
            if (result == null)
                return NotFound(new { message = "User not found." });

            return Ok(result);
        }

        // ── LISTING MANAGEMENT ────────────────────────────────────────

        // GET /api/admin/listings
        /// <summary>Returns all listings across all universities — Admin only.</summary>
        [HttpGet("listings")]
        public async Task<IActionResult> GetAllListings()
        {
            if (!IsAdmin())
                return Forbid();

            var listings = await _adminService.GetAllListingsAsync();
            return Ok(listings);
        }

        // DELETE /api/admin/listings/{id}
        /// <summary>Soft deletes a listing — Admin only.</summary>
        [HttpDelete("listings/{id}")]
        public async Task<IActionResult> RemoveListing(int id)
        {
            if (!IsAdmin())
                return Forbid();

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var success = await _adminService.RemoveListingAsync(id, userId.Value);
            if (!success)
                return NotFound(new { message = "Listing not found." });

            return NoContent();
        }

        // PUT /api/admin/listings/{id}/featured
        /// <summary>Toggles the featured status of a listing — Admin only.</summary>
        [HttpPut("listings/{id}/featured")]
        public async Task<IActionResult> ToggleFeatured(int id)
        {
            if (!IsAdmin())
                return Forbid();

            var success = await _adminService.ToggleFeaturedAsync(id);
            if (!success)
                return NotFound(new { message = "Listing not found." });

            return Ok(new { message = "Featured status toggled." });
        }

        // ── REPORTS ───────────────────────────────────────────────────

        // GET /api/admin/reports
        /// <summary>Returns all reports in the reports queue — Admin only.</summary>
        [HttpGet("reports")]
        public async Task<IActionResult> GetAllReports()
        {
            if (!IsAdmin())
                return Forbid();

            var reports = await _adminService.GetAllReportsAsync();
            return Ok(reports);
        }

        // GET /api/admin/reports/{id}
        /// <summary>Returns a single report by ID — Admin only.</summary>
        [HttpGet("reports/{id}")]
        public async Task<IActionResult> GetReportById(int id)
        {
            if (!IsAdmin())
                return Forbid();

            var report = await _adminService.GetReportByIdAsync(id);
            if (report == null)
                return NotFound(new { message = "Report not found." });

            return Ok(report);
        }

        // PUT /api/admin/reports/{id}/resolve
        /// <summary>Resolves or dismisses a report — Admin only.</summary>
        [HttpPut("reports/{id}/resolve")]
        public async Task<IActionResult> ResolveReport(int id, [FromBody] ResolveReportDto request)
        {
            if (!IsAdmin())
                return Forbid();

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized();

            var success = await _adminService.ResolveReportAsync(id, userId.Value, request);
            if (!success)
                return NotFound(new { message = "Report not found." });

            return Ok(new { message = "Report resolved." });
        }

        // ── HERO BANNER ───────────────────────────────────────────────

        // GET /api/admin/banner
        /// <summary>Returns all hero banner slides ordered by sort order — Admin only.</summary>
        [HttpGet("banner")]
        public async Task<IActionResult> GetAllSlides()
        {
            if (!IsAdmin())
                return Forbid();

            var slides = await _adminService.GetAllSlidesAsync();
            return Ok(slides);
        }

        // POST /api/admin/banner
        /// <summary>Creates a new hero banner slide — Admin only.</summary>
        [HttpPost("banner")]
        public async Task<IActionResult> CreateSlide([FromBody] HeroBannerSlideRequestDto request)
        {
            if (!IsAdmin())
                return Forbid();

            var result = await _adminService.CreateSlideAsync(request);
            if (result == null)
                return BadRequest(new { message = "Could not create slide." });

            return Ok(result);
        }

        // PUT /api/admin/banner/{id}
        /// <summary>Updates an existing hero banner slide — Admin only.</summary>
        [HttpPut("banner/{id}")]
        public async Task<IActionResult> UpdateSlide(int id, [FromBody] HeroBannerSlideRequestDto request)
        {
            if (!IsAdmin())
                return Forbid();

            var result = await _adminService.UpdateSlideAsync(id, request);
            if (result == null)
                return NotFound(new { message = "Slide not found." });

            return Ok(result);
        }

        // DELETE /api/admin/banner/{id}
        /// <summary>Deletes a hero banner slide — Admin only.</summary>
        [HttpDelete("banner/{id}")]
        public async Task<IActionResult> DeleteSlide(int id)
        {
            if (!IsAdmin())
                return Forbid();

            var success = await _adminService.DeleteSlideAsync(id);
            if (!success)
                return NotFound(new { message = "Slide not found." });

            return NoContent();
        }

        // PUT /api/admin/banner/{id}/visibility
        /// <summary>Toggles the visibility of a hero banner slide — Admin only.</summary>
        [HttpPut("banner/{id}/visibility")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            if (!IsAdmin())
                return Forbid();

            var success = await _adminService.ToggleSlideVisibilityAsync(id);
            if (!success)
                return NotFound(new { message = "Slide not found." });

            return Ok(new { message = "Slide visibility toggled." });
        }

        // ── SYSTEM SETTINGS ───────────────────────────────────────────

        // GET /api/admin/settings
        /// <summary>Returns all system settings — Admin only.</summary>
        [HttpGet("settings")]
        public async Task<IActionResult> GetSystemSettings()
        {
            if (!IsAdmin())
                return Forbid();

            var settings = await _adminService.GetSystemSettingsAsync();
            return Ok(settings);
        }

        // PUT /api/admin/settings/{key}
        /// <summary>Updates a system setting value by key — Admin only.</summary>
        [HttpPut("settings/{key}")]
        public async Task<IActionResult> UpdateSystemSetting(string key, [FromBody] string value)
        {
            if (!IsAdmin())
                return Forbid();

            var success = await _adminService.UpdateSystemSettingAsync(key, value);
            if (!success)
                return NotFound(new { message = $"Setting '{key}' not found." });

            return Ok(new { message = $"Setting '{key}' updated to '{value}'." });
        }

        // ── PLATFORM STATS ────────────────────────────────────────────

        // GET /api/admin/stats
        /// <summary>Returns platform-wide statistics across all universities — Admin only.</summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetPlatformStats()
        {
            if (!IsAdmin())
                return Forbid();

            var stats = await _adminService.GetPlatformStatsAsync();
            return Ok(stats);
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetRecentActivity()
        {
            if (!IsAdmin())
                return Forbid();

            var activity =
                await _adminService.GetRecentAuditLogsAsync();

            return Ok(activity);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/admin/public-stats
        // Read-only statistics used by the public homepage.
        // This does NOT expose admin management functionality.
        // ─────────────────────────────────────────────────────────────
        [AllowAnonymous]
        [HttpGet("public-stats")]
        public async Task<IActionResult> GetPublicStats()
        {
            var stats = await _adminService.GetPlatformStatsAsync();

            return Ok(new
            {
                activeStudents = stats.ActiveUsers,
                itemsListed = stats.ActiveListings,
                tradesDone = stats.CompletedTransactions,
                verificationRate = stats.VerificationRate
            });
        }
    }
}