using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using VarsityTrade.Web.Models.Listings; // Provides Listing view models
using VarsityTrade.Web.Services; // Provides ApiService
using System.Text.Json; // Provides JsonElement for safe API response mapping

namespace VarsityTrade.Web.Controllers
{
    // ListingsController handles all listing-related pages
    // Browse, Detail, Search, and Category pages
    [Route("listings")]
    public class ListingsController : Controller
    {
        // ApiService handles all HTTP calls to the backend API
        private readonly ApiService _api;

        public ListingsController(ApiService api)
        {
            _api = api;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /listings/browse
        // Shows all active listings for the logged in user's university
        // Campus-locked for authenticated users
        // ─────────────────────────────────────────────────────────────
        [HttpGet("browse")]
        public async Task<IActionResult> Browse(string? q, string? category, string? condition)
        {
            var universityIdStr = HttpContext.Session.GetString("UniversityId");
            var universityId = int.TryParse(universityIdStr, out var uid) ? uid : 1;

            // Fetch raw listings from API
            var raw = await _api.GetAsync<List<System.Text.Json.JsonElement>>(
                $"api/listings/university/{universityId}");

            // Map to view model — handles different field name casings from the API
            var listings = raw?.Select(l => new ListingCardViewModel
            {
                ListingId = l.TryGetProperty("listingId", out var lid) ? lid.GetInt32() : 0,
                Title = l.TryGetProperty("title", out var t) ? t.GetString()! : "",
                Price = l.TryGetProperty("price", out var p) ? p.GetDecimal() : 0,
                Condition = l.TryGetProperty("condition", out var cond) ? cond.GetString()! : "",
                CategoryName = l.TryGetProperty("categoryName", out var cat) ? cat.GetString()! : "",
                Status = l.TryGetProperty("status", out var st) ? st.GetString()! : "",
                StoreName = l.TryGetProperty("storeName", out var sn) ? sn.GetString()! : "",
                ViewCount = l.TryGetProperty("viewCount", out var vc) ? vc.GetInt32() : 0,
                IsFeatured = l.TryGetProperty("isFeatured", out var feat) ? feat.GetBoolean() : false,
                UniversityShortName = l.TryGetProperty("universityShortName", out var us) ? us.GetString()! : "",
                CreatedAt = l.TryGetProperty("createdAt", out var ca) ? ca.GetDateTime() : DateTime.UtcNow,
            }).ToList() ?? new List<ListingCardViewModel>();

            // Get university name for campus lock banner
            var universityName = await GetUniversityNameAsync(universityId);

            var model = new BrowseViewModel
            {
                Listings = listings,
                UniversityName = universityName,
                SearchQuery = q,
                SelectedCategory = category,
                SelectedCondition = condition,
            };

            if (!string.IsNullOrEmpty(q))
                model.Listings = model.Listings
                    .Where(l => l.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                             || l.CategoryName.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(category))
                model.Listings = model.Listings
                    .Where(l => l.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(condition))
                model.Listings = model.Listings
                    .Where(l => l.Condition.Equals(condition, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            ViewData["SidebarPage"] = "browse";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /listings/{id}
        // Shows the full detail page for a single listing
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            // Fetch the listing from the API — also increments the view count
            var listing = await _api.GetAsync<ListingDetailViewModel>($"api/listings/{id}");

            if (listing == null)
                return NotFound();

            ViewData["SidebarPage"] = "browse";
            return View(listing);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /listings/search
        // Searches listings by keyword
        // ─────────────────────────────────────────────────────────────
        [HttpGet("search")]
        public async Task<IActionResult> Search(string? q)
        {
            // Redirect to browse with the search query applied
            return RedirectToAction("Browse", new { q });
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        // Gets the university name from the API for the campus lock banner
        private async Task<string> GetUniversityNameAsync(int universityId)
        {
            var universities = await _api.GetAsync<List<dynamic>>("api/universities");
            var uni = universities?.FirstOrDefault(u => u.universityId == universityId);
            return uni?.name ?? "Your University";
        }
    }
}