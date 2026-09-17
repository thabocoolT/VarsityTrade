using Microsoft.AspNetCore.Mvc;
using VarsityTrade.Web.Models.Home;
using VarsityTrade.Web.Models.Listings;
using VarsityTrade.Web.Models.Seller;
using VarsityTrade.Web.Services;

namespace VarsityTrade.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            _api = api;
        }

        // GET /
        public async Task<IActionResult> Index()
        {
            // Check whether the user is logged in
            var accessToken = HttpContext.Session.GetString("AccessToken");
            var universityIdStr = HttpContext.Session.GetString("UniversityId");

            List<ListingCardViewModel> allListings;

            // ============================================================
            // LOGGED-IN USER
            // Only show listings from the user's university
            // ============================================================
            if (!string.IsNullOrWhiteSpace(accessToken) &&
                int.TryParse(universityIdStr, out var universityId))
            {
                var listings =
                    await _api.GetAsync<List<ListingCardViewModel>>(
                        $"api/listings/university/{universityId}");

                allListings =
                    listings ?? new List<ListingCardViewModel>();
            }
            // ============================================================
            // GUEST USER
            // Show listings from ALL universities
            // ============================================================
            else
            {
                var listings =
                    await _api.GetAsync<List<ListingCardViewModel>>(
                        "api/listings");

                allListings =
                    listings ?? new List<ListingCardViewModel>();
            }

            // ============================================================
            // RECENT LISTINGS
            // ============================================================
            ViewBag.RecentListings =
                allListings
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(6)
                    .ToList();

            // ============================================================
            // FEATURED LISTINGS
            // ============================================================
            ViewBag.FeaturedListings =
                allListings
                    .Where(l => l.IsFeatured)
                    .OrderByDescending(l => l.CreatedAt)
                    .Take(3)
                    .ToList();

            // ============================================================
            // LOAD REAL REVIEWS
            // ============================================================
            var sellerProfileIds =
                allListings
                    .Where(l => l.SellerProfileId > 0)
                    .Select(l => l.SellerProfileId)
                    .Distinct()
                    .ToList();

            var homepageReviews =
                new List<SellerDashboardViewModel.SellerReviewViewModel>();

            foreach (var sellerProfileId in sellerProfileIds)
            {
                var sellerReviews =
                    await _api.GetAsync<List<SellerDashboardViewModel.SellerReviewViewModel>>(
                        $"api/reviews/seller/{sellerProfileId}");

                if (sellerReviews != null)
                {
                    homepageReviews.AddRange(sellerReviews);
                }
            }

            ViewBag.Reviews =
                homepageReviews
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(3)
                    .ToList();

            return View();
        }
    }
}