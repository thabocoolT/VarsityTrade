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
        public async Task<IActionResult> Browse(
            string? q,
            string? category,
            string? condition)
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            var isLoggedIn = !string.IsNullOrWhiteSpace(accessToken);

            List<ListingCardViewModel> listings = new();
            string universityName = "All Universities";

            if (isLoggedIn)
            {
                // Logged-in users are restricted to their university.
                var universityIdStr =
                    HttpContext.Session.GetString("UniversityId");

                if (!int.TryParse(universityIdStr, out var universityId))
                {
                    return View(new BrowseViewModel
                    {
                        Listings = new(),
                        UniversityName = "Your University",
                        SearchQuery = q,
                        SelectedCategory = category,
                        SelectedCondition = condition
                    });
                }

                universityName =
                    await GetUniversityNameAsync(universityId);

                listings =
                    await _api.GetAsync<List<ListingCardViewModel>>(
                        $"api/listings/university/{universityId}")
                    ?? new List<ListingCardViewModel>();
            }
            else
            {
                // Guests can browse listings from all universities.
                listings =
                    await _api.GetAsync<List<ListingCardViewModel>>(
                        "api/listings/public")
                    ?? new List<ListingCardViewModel>();
            }

            var model = new BrowseViewModel
            {
                Listings = listings,
                UniversityName = universityName,
                SearchQuery = q,
                SelectedCategory = category,
                SelectedCondition = condition
            };

            // ─────────────────────────────────────────────────────────
            // Search
            // ─────────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(q))
            {
                model.Listings = model.Listings
                    .Where(l =>
                        (!string.IsNullOrWhiteSpace(l.Title) &&
                         l.Title.Contains(
                             q,
                             StringComparison.OrdinalIgnoreCase))
                        ||
                        (!string.IsNullOrWhiteSpace(l.CategoryName) &&
                         l.CategoryName.Contains(
                             q,
                             StringComparison.OrdinalIgnoreCase))
                        ||
                        (!string.IsNullOrWhiteSpace(l.StoreName) &&
                         l.StoreName.Contains(
                             q,
                             StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            // ─────────────────────────────────────────────────────────
            // Category filter
            // ─────────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(category))
            {
                model.Listings = model.Listings
                    .Where(l =>
                        string.Equals(
                            l.CategoryName,
                            category,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // ─────────────────────────────────────────────────────────
            // Condition filter
            // ─────────────────────────────────────────────────────────
            if (!string.IsNullOrWhiteSpace(condition))
            {
                model.Listings = model.Listings
                    .Where(l =>
                        string.Equals(
                            l.Condition,
                            condition,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewData["SidebarPage"] = "browse";

            return View(model);
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