using Microsoft.AspNetCore.Mvc;
using VarsityTrade.Web.Models.Listings;
using VarsityTrade.Web.Services;

namespace VarsityTrade.Web.Controllers
{
    [Route("listings")]
    public class ListingsController : Controller
    {
        private readonly ApiService _api;

        public ListingsController(ApiService api)
        {
            _api = api;
        }

        // GET /listings/browse
        [HttpGet("browse")]
        public async Task<IActionResult> Browse(
            string? q,
            string? category,
            string? condition)
        {
            var isLoggedIn =
                !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("AccessToken"));

            List<ListingCardViewModel> listings;

            string universityName;

            if (isLoggedIn)
            {
                var universityIdStr =
                    HttpContext.Session.GetString("UniversityId");

                if (!int.TryParse(
                    universityIdStr,
                    out var universityId))
                {
                    return RedirectToAction(
                        "Login",
                        "Auth");
                }

                listings =
                    await _api.GetAsync<
                        List<ListingCardViewModel>>(
                            $"api/listings/university/{universityId}")
                    ?? new List<ListingCardViewModel>();

                universityName =
                    await GetUniversityNameAsync(
                        universityId);
            }
            else
            {
                listings =
                    await _api.GetAsync<
                        List<ListingCardViewModel>>(
                            "api/listings/public")
                    ?? new List<ListingCardViewModel>();

                universityName =
                    "All South African Universities";
            }

            // Search real listing fields.
            if (!string.IsNullOrWhiteSpace(q))
            {
                var searchTerm =
                    q.Trim();

                listings =
                    listings
                        .Where(l =>
                            l.Title.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            l.Description.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            l.CategoryName.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            l.Condition.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            l.StoreName.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)
                            ||
                            l.ListingType.Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                listings =
                    listings
                        .Where(l =>
                            l.CategoryName.Equals(
                                category,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }

            if (!string.IsNullOrWhiteSpace(condition))
            {
                listings =
                    listings
                        .Where(l =>
                            l.Condition.Equals(
                                condition,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();
            }

            // Load real categories for the browse filter.
            var categoryData =
                await _api.GetAsync<
                    List<VarsityTrade.Web.Models.Seller.CategoryOption>>(
                        "api/categories");

            var categories =
                categoryData?
                    .Select(c => c.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n)
                    .ToList()
                ?? new List<string>();

            var model = new BrowseViewModel
            {
                Listings = listings,
                UniversityName = universityName,
                SearchQuery = q,
                SelectedCategory = category,
                SelectedCondition = condition,
                Categories = categories
            };

            ViewData["SidebarPage"] = "browse";

            return View(model);
        }

        // GET /listings/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> Detail(int id)
        {
            var listing =
                await _api.GetAsync<ListingDetailViewModel>(
                    $"api/listings/{id}");

            if (listing == null)
                return NotFound();

            // Determine whether the logged-in user owns this listing.
            ViewBag.IsOwner = false;

            if (!string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("AccessToken")))
            {
                var profile =
                    await _api.GetAsync<dynamic>(
                        "api/sellerprofiles/my");

                if (profile != null)
                {
                    var sellerProfileId =
                        (int?)(profile.sellerProfileId ?? 0);

                    ViewBag.IsOwner =
                        sellerProfileId ==
                        listing.SellerProfileId;
                }
            }

            ViewData["SidebarPage"] = "browse";

            return View(listing);
        }

        // GET /listings/search
        [HttpGet("search")]
        public IActionResult Search(string? q)
        {
            return RedirectToAction(
                "Browse",
                new
                {
                    q
                });
        }

        private async Task<string>
            GetUniversityNameAsync(
                int universityId)
        {
            var universities =
                await _api.GetAsync<List<dynamic>>(
                    "api/universities");

            var university =
                universities?
                    .FirstOrDefault(
                        u =>
                            (int)u.universityId ==
                            universityId);

            return university?.name
                   ?? "Your University";
        }
    }
}