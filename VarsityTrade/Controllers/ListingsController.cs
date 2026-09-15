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

            // Fetch listings directly as the concrete view model
            // Newtonsoft.Json handles camelCase → PascalCase automatically
            var listings = await _api.GetAsync<List<ListingCardViewModel>>(
                $"api/listings/university/{universityId}") ?? new List<ListingCardViewModel>();

            // Apply filters
            if (!string.IsNullOrEmpty(q))
                listings = listings
                    .Where(l => l.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
                             || l.CategoryName.Contains(q, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(category))
                listings = listings
                    .Where(l => l.CategoryName.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            if (!string.IsNullOrEmpty(condition))
                listings = listings
                    .Where(l => l.Condition.Equals(condition, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var universityName = await GetUniversityNameAsync(universityId);

            var model = new BrowseViewModel
            {
                Listings = listings,
                UniversityName = universityName,
                SearchQuery = q,
                SelectedCategory = category,
                SelectedCondition = condition,
            };

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