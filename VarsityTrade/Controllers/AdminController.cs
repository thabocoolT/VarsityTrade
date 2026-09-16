using Microsoft.AspNetCore.Mvc;
using VarsityTrade.Web.Models.Admin;
using VarsityTrade.Web.Services;

namespace VarsityTrade.Web.Controllers
{
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly ApiService _api;

        public AdminController(ApiService api)
        {
            _api = api;
        }

        // Require admin role — redirects to login if not admin
        private IActionResult? RequireAdmin()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToAction("Login", "Auth");
            if (HttpContext.Session.GetString("UserRole") != "Admin")
                return RedirectToAction("Index", "Home");
            return null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /admin — Admin Dashboard
        // ─────────────────────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            var stats = await _api.GetAsync<PlatformStatsViewModel>("api/admin/stats")
                          ?? new PlatformStatsViewModel();
            var reports = await _api.GetAsync<List<AdminReportViewModel>>("api/admin/reports")
                          ?? new List<AdminReportViewModel>();
            var users = await _api.GetAsync<List<AdminUserViewModel>>("api/admin/users")
                          ?? new List<AdminUserViewModel>();

            var model = new AdminDashboardViewModel
            {
                Stats = stats,
                OpenReports = reports.Where(r => r.Status == "Open" || r.Status == "UnderReview")
                                     .Take(4).ToList(),
                RecentUsers = users.OrderByDescending(u => u.CreatedAt).Take(4).ToList(),
            };

            ViewData["SidebarPage"] = "admin-dashboard";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /admin/users — Manage Users
        // ─────────────────────────────────────────────────────────────
        [HttpGet("users")]
        public async Task<IActionResult> Users(string filter = "All")
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            var users = await _api.GetAsync<List<AdminUserViewModel>>("api/admin/users")
                        ?? new List<AdminUserViewModel>();

            var filtered = filter switch
            {
                "Active" => users.Where(u => u.IsActive && !u.IsBanned).ToList(),
                "Banned" => users.Where(u => u.IsBanned).ToList(),
                "Deactivated" => users.Where(u => !u.IsActive && !u.IsBanned).ToList(),
                "New" => users.Where(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-7)).ToList(),
                _ => users
            };

            ViewBag.Users = filtered;
            ViewBag.ActiveFilter = filter;
            ViewBag.TotalCount = users.Count;
            ViewBag.ActiveCount = users.Count(u => u.IsActive && !u.IsBanned);
            ViewBag.BannedCount = users.Count(u => u.IsBanned);
            ViewData["SidebarPage"] = "admin-users";
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // POST /admin/users/ban — Ban a user
        // ─────────────────────────────────────────────────────────────
        [HttpPost("users/ban")]
        public async Task<IActionResult> BanUser(int userId)
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            await _api.PutAsync<object>($"api/admin/users/{userId}", new
            {
                isActive = false,
                isBanned = true,
                studentVerified = true
            });

            return RedirectToAction("Users");
        }

        // ─────────────────────────────────────────────────────────────
        // POST /admin/users/unban — Unban a user
        // ─────────────────────────────────────────────────────────────
        [HttpPost("users/unban")]
        public async Task<IActionResult> UnbanUser(int userId)
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            await _api.PutAsync<object>($"api/admin/users/{userId}", new
            {
                isActive = true,
                isBanned = false,
                studentVerified = true
            });

            return RedirectToAction("Users");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /admin/listings — Manage Listings
        // ─────────────────────────────────────────────────────────────
        [HttpGet("listings")]
        public async Task<IActionResult> Listings(string filter = "All")
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            var listings = await _api.GetAsync<List<AdminListingViewModel>>("api/admin/listings")
                           ?? new List<AdminListingViewModel>();

            var filtered = filter switch
            {
                "Active" => listings.Where(l => l.Status == "Active").ToList(),
                "Sold" => listings.Where(l => l.Status == "Sold").ToList(),
                "Flagged" => listings.Where(l => l.DeletedAt != null).ToList(),
                _ => listings
            };

            ViewBag.Listings = filtered;
            ViewBag.ActiveFilter = filter;
            ViewBag.TotalCount = listings.Count;
            ViewBag.ActiveCount = listings.Count(l => l.Status == "Active");
            ViewBag.SoldCount = listings.Count(l => l.Status == "Sold");
            ViewBag.DeletedCount = listings.Count(l => l.DeletedAt != null);
            ViewData["SidebarPage"] = "admin-listings";
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // POST /admin/listings/remove — Remove a listing
        // ─────────────────────────────────────────────────────────────
        [HttpPost("listings/remove")]
        public async Task<IActionResult> RemoveListing(int listingId)
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            await _api.DeleteAsync($"api/admin/listings/{listingId}");
            return RedirectToAction("Listings");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /admin/reports — Reports Queue
        // ─────────────────────────────────────────────────────────────
        [HttpGet("reports")]
        public async Task<IActionResult> Reports(string filter = "All")
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            var reports = await _api.GetAsync<List<AdminReportViewModel>>("api/admin/reports")
                          ?? new List<AdminReportViewModel>();

            var filtered = filter switch
            {
                "Urgent" => reports.Where(r => r.ReportType == "Scam" || r.Status == "Urgent").ToList(),
                "Review" => reports.Where(r => r.Status == "UnderReview").ToList(),
                "Resolved" => reports.Where(r => r.Status == "Resolved" || r.Status == "Dismissed").ToList(),
                _ => reports.Where(r => r.Status == "Open" || r.Status == "UnderReview").ToList()
            };

            ViewBag.Reports = filtered;
            ViewBag.ActiveFilter = filter;
            ViewBag.OpenCount = reports.Count(r => r.Status == "Open");
            ViewBag.TotalCount = reports.Count;
            ViewData["SidebarPage"] = "admin-reports";
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // POST /admin/reports/resolve — Resolve a report
        // ─────────────────────────────────────────────────────────────
        [HttpPost("reports/resolve")]
        public async Task<IActionResult> ResolveReport(int reportId, string status, string? notes)
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            await _api.PutAsync<object>($"api/admin/reports/{reportId}/resolve", new
            {
                status = status,
                adminNotes = notes
            });

            return RedirectToAction("Reports");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /admin/stats — Platform Statistics
        // ─────────────────────────────────────────────────────────────
        [HttpGet("stats")]
        public async Task<IActionResult> Stats()
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            var stats = await _api.GetAsync<PlatformStatsViewModel>("api/admin/stats")
                        ?? new PlatformStatsViewModel();

            ViewData["SidebarPage"] = "admin-stats";
            return View(stats);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /admin/banner — Hero Banner Manager
        // ─────────────────────────────────────────────────────────────
        [HttpGet("banner")]
        public async Task<IActionResult> Banner()
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            var slides = await _api.GetAsync<List<HeroBannerSlideViewModel>>("api/admin/banner")
                         ?? new List<HeroBannerSlideViewModel>();

            ViewBag.Slides = slides;
            ViewData["SidebarPage"] = "admin-banner";
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // POST /admin/banner/toggle — Toggle slide visibility
        // ─────────────────────────────────────────────────────────────
        [HttpPost("banner/toggle")]
        public async Task<IActionResult> ToggleSlide(int slideId)
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            await _api.PutAsync($"api/admin/banner/{slideId}/visibility");
            return RedirectToAction("Banner");
        }

        // ─────────────────────────────────────────────────────────────
        // POST /admin/banner/delete — Delete a slide
        // ─────────────────────────────────────────────────────────────
        [HttpPost("banner/delete")]
        public async Task<IActionResult> DeleteSlide(int slideId)
        {
            var auth = RequireAdmin();
            if (auth != null) return auth;

            await _api.DeleteAsync($"api/admin/banner/{slideId}");
            return RedirectToAction("Banner");
        }
    }
}