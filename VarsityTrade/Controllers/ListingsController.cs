using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using VarsityTrade.Web.Models.Listings; // Provides Listing view models
using VarsityTrade.Web.Services; // Provides ApiService

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
            // Get the university ID from session — set at login
            var universityIdStr = HttpContext.Session.GetString("UniversityId");

            // If not logged in use university 1 as default for guest browsing
            var universityId = int.TryParse(universityIdStr, out var uid) ? uid : 1;

            // Get the university name for the campus lock banner
            var universityName = await GetUniversityNameAsync(universityId);

            // Fetch listings from the API for this university
            var listings = await _api.GetAsync<List<ListingCardViewModel>>(
                $"api/listings/university/{universityId}");

            var model = new BrowseViewModel
            {
                Listings = listings ?? new List<ListingCardViewModel>(),
                UniversityName = universityName,
                SearchQuery = q,
                SelectedCategory = category,
                SelectedCondition = condition,
            };

            // Apply client-side filters if provided
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