using Microsoft.AspNetCore.Mvc;
using VarsityTrade.Web.Models.Listings;
using VarsityTrade.Web.Services;

namespace VarsityTrade.Web.Controllers
{
    // Handles public and authenticated listing pages.
    [Route("listings")]
    public class ListingsController : Controller
    {
        private readonly ApiService _api;

        public ListingsController(ApiService api)
        {
            _api = api;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /listings/browse
        // ─────────────────────────────────────────────────────────────
        [HttpGet("browse")]
        public async Task<IActionResult> Browse(string? q, string? category, string? condition)
        {
            var universityIdStr = HttpContext.Session.GetString("UniversityId");
            var isLoggedIn = HttpContext.Session.GetString("AccessToken") != null;

            List<ListingCardViewModel> listings;

            if (isLoggedIn && int.TryParse(universityIdStr, out var uid))
            {
                // Authenticated — campus-locked feed
                listings = await _api.GetAsync<List<ListingCardViewModel>>(
                    $"api/listings/university/{uid}") ?? new();
            }
            else
            {
                // Guest — show all listings from university 1 as default
                // In production this would show a combined feed across universities
                listings = await _api.GetAsync<List<ListingCardViewModel>>(
                    "api/listings/university/1") ?? new();
            }

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

            var universityName = isLoggedIn
                ? await GetUniversityNameAsync(int.Parse(universityIdStr ?? "1"))
                : "South African Universities";

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

        // Helper to get raw JSON from API for debugging
        private async Task<string?> GetRawJsonAsync(string endpoint)
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                using var client = new HttpClient();
                if (!string.IsNullOrEmpty(token))
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var baseUrl = _api.GetType()
                    .GetField("_apiBaseUrl",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(_api)?.ToString() ?? "https://localhost:7019";

                var response = await client.GetAsync($"{baseUrl}/{endpoint}");
                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                return null;
            }
        }

        // GET /listings/edit/{id} — redirects to seller edit page
        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            return RedirectToAction("EditListing", "Seller", new { id });
        }

        // ─────────────────────────────────────────────────────────────
        // GET /listings/{id}
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var listing =
                await _api.GetAsync<ListingDetailViewModel>(
                    $"api/listings/{id}");

            if (listing == null)
            {
                return NotFound();
            }

            // ---------------------------------------------------------
            // Determine whether the logged-in user owns this listing.
            // ---------------------------------------------------------
            var accessToken =
                HttpContext.Session.GetString("AccessToken");

            var currentMode =
                HttpContext.Session.GetString("CurrentMode") ?? "Buyer";

            var isSellerMode =
                currentMode.Equals(
                    "Seller",
                    StringComparison.OrdinalIgnoreCase);

            var isOwner = false;

            if (!string.IsNullOrWhiteSpace(accessToken) && isSellerMode)
            {
                var sellerProfile =
                    await _api.GetAsync<dynamic>(
                        "api/sellerprofiles/my");

                if (sellerProfile != null)
                {
                    try
                    {
                        int sellerProfileId =
                            (int)sellerProfile.sellerProfileId;

                        isOwner =
                            sellerProfileId == listing.SellerProfileId;
                    }
                    catch
                    {
                        isOwner = false;
                    }
                }
            }

            // ---------------------------------------------------------
            // Related listings
            // ---------------------------------------------------------
            var relatedListings =
                new List<ListingCardViewModel>();

            var universityIdStr =
                HttpContext.Session.GetString("UniversityId");

            if (int.TryParse(
                    universityIdStr,
                    out var universityId))
            {
                var universityListings =
                    await _api.GetAsync<List<ListingCardViewModel>>(
                        $"api/listings/university/{universityId}");

                if (universityListings != null)
                {
                    relatedListings = universityListings
                        .Where(l =>
                            l.ListingId != listing.ListingId &&
                            (
                                string.Equals(
                                    l.CategoryName,
                                    listing.CategoryName,
                                    StringComparison.OrdinalIgnoreCase)
                                ||
                                l.CategoryName.Contains(
                                    listing.CategoryName,
                                    StringComparison.OrdinalIgnoreCase)
                                ||
                                listing.CategoryName.Contains(
                                    l.CategoryName,
                                    StringComparison.OrdinalIgnoreCase)
                            ))
                        .OrderByDescending(l => l.IsFeatured)
                        .ThenByDescending(l => l.CreatedAt)
                        .Take(4)
                        .ToList();
                }
            }

            // If fewer than four category matches exist,
            // fill the remaining slots with other listings.
            if (relatedListings.Count < 4)
            {
                var universityIdForFallback =
                    int.TryParse(
                        universityIdStr,
                        out var fallbackUniversityId)
                        ? fallbackUniversityId
                        : 0;

                if (universityIdForFallback > 0)
                {
                    var universityListings =
                        await _api.GetAsync<List<ListingCardViewModel>>(
                            $"api/listings/university/{universityIdForFallback}");

                    if (universityListings != null)
                    {
                        var existingIds =
                            relatedListings
                                .Select(l => l.ListingId)
                                .ToHashSet();

                        var additionalListings =
                            universityListings
                                .Where(l =>
                                    l.ListingId != listing.ListingId &&
                                    !existingIds.Contains(l.ListingId))
                                .OrderByDescending(l => l.IsFeatured)
                                .ThenByDescending(l => l.CreatedAt)
                                .Take(4 - relatedListings.Count)
                                .ToList();

                        relatedListings.AddRange(additionalListings);
                    }
                }
            }

            ViewBag.IsOwner = isOwner;
            ViewBag.RelatedListings = relatedListings;

            ViewData["SidebarPage"] = "browse";

            return View(listing);
        }

        [HttpGet("/seller/{id:int}")]
        public async Task<IActionResult> SellerProfile(int id)
        {
            var profile = await _api.GetAsync<dynamic>($"api/sellerprofiles/{id}");
            if (profile == null) return NotFound();

            var reviews = await _api.GetAsync<List<VarsityTrade.Web.Models.Reviews.ReviewViewModel>>(
                $"api/reviews/seller/{id}") ?? new();

            ViewBag.Profile = profile;
            ViewBag.Reviews = reviews;
            ViewBag.AvgRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // GET /listings/search
        // ─────────────────────────────────────────────────────────────
        [HttpGet("search")]
        public async Task<IActionResult> Search(string? q)
        {
            return RedirectToAction(
                nameof(Browse),
                new { q });
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        private async Task<string> GetUniversityNameAsync(int universityId)
        {
            var universities =
                await _api.GetAsync<List<dynamic>>(
                    "api/universities");

            if (universities == null)
            {
                return "Your University";
            }

            var university =
                universities.FirstOrDefault(
                    u => (int)u.universityId == universityId);

            return university?.name ?? "Your University";
        }
    }
}