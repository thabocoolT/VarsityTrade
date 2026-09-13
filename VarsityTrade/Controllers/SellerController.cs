using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using VarsityTrade.Web.Models.Seller; // Provides Seller view models
using VarsityTrade.Web.Services; // Provides ApiService

namespace VarsityTrade.Web.Controllers
{
    // SellerController handles all seller dashboard pages
    // Requires the user to have an active seller profile
    [Route("seller")]
    public class SellerController : Controller
    {
        // ApiService handles all HTTP calls to the backend API
        private readonly ApiService _api;

        public SellerController(ApiService api)
        {
            _api = api;
        }

        // Helper — redirects to login if not authenticated
        private IActionResult? RequireAuth()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToAction("Login", "Auth");
            return null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /seller
        // Seller dashboard — shows stats, recent listings, pending offers
        // ─────────────────────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Load the seller profile
            var profile = await _api.GetAsync<dynamic>("api/sellerprofiles/my");
            if (profile == null)
                return RedirectToAction("Activate");

            // Load listings
            var listings = await _api.GetAsync<List<SellerListingViewModel>>(
                $"api/listings/university/{HttpContext.Session.GetString("UniversityId")}");

            // Load received offers
            var offers = await _api.GetAsync<List<ReceivedOfferViewModel>>("api/offers/received");

            // Load conversations for unread count
            var conversations = await _api.GetAsync<List<dynamic>>("api/messaging/conversations");

            var model = new SellerDashboardViewModel
            {
                StoreName = profile?.storeName ?? "My Shop",
                AverageRating = (decimal)(profile?.averageRating ?? 0),
                TotalSales = (int)(profile?.totalSales ?? 0),
                IsActive = (bool)(profile?.isActive ?? true),
                ActiveListingsCount = listings?.Count ?? 0,
                PendingOffersCount = offers?.Count(o => o.Status == "Pending") ?? 0,
                UnreadMessagesCount = conversations?.Sum(c => (int)(c?.unreadCount ?? 0)) ?? 0,
                RecentListings = listings?.Take(4).ToList() ?? new(),
                PendingOffers = offers?.Where(o => o.Status == "Pending").Take(3).ToList() ?? new(),
            };

            // Store seller profile status in session for sidebar
            HttpContext.Session.SetString("HasSellerProfile", "true");

            ViewData["SidebarPage"] = "seller-dashboard";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /seller/listings
        // My Listings — shows all the seller's listings
        // ─────────────────────────────────────────────────────────────
        [HttpGet("listings")]
public async Task<IActionResult> Listings(string filter = "All")
{
    var auth = RequireAuth();
    if (auth != null) return auth;

    // Make sure the user actually has a seller profile
    var profile = await _api.GetAsync<dynamic>("api/sellerprofiles/my");

    if (profile == null)
    {
        Response.Cookies.Delete("HasSellerProfile");
        HttpContext.Session.Remove("HasSellerProfile");

        return RedirectToAction("Activate");
    }

    // Remember seller capability
    HttpContext.Session.SetString("HasSellerProfile", "true");

    Response.Cookies.Append(
        "HasSellerProfile",
        "true",
        new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.AddDays(30)
        });

    var universityId = HttpContext.Session.GetString("UniversityId") ?? "1";

    var listings = await _api.GetAsync<List<SellerListingViewModel>>(
        $"api/listings/university/{universityId}");

    var filtered = listings ?? new List<SellerListingViewModel>();

    if (filter != "All")
    {
        filtered = filtered
            .Where(l => l.Status.Equals(
                filter,
                StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    ViewBag.Listings = filtered;
    ViewBag.ActiveFilter = filter;
    ViewData["SidebarPage"] = "seller-listings";

    // IMPORTANT:
    // The actual view is inside Views/Listings/
    return View("~/Views/Listings/Listings.cshtml");
}

        // ─────────────────────────────────────────────────────────────
        // GET /seller/listings/create
        // Create Listing page
        // ─────────────────────────────────────────────────────────────
        [HttpGet("listings/create")]
        public async Task<IActionResult> CreateListing()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var model = new CreateListingViewModel
            {
                Categories = await GetCategoriesAsync(),
                Conditions = await GetConditionsAsync(),
            };

            ViewData["SidebarPage"] = "seller-listings";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /seller/listings/create
        // Submits a new listing to the API
        // ─────────────────────────────────────────────────────────────
        [HttpPost("listings/create")]
        public async Task<IActionResult> CreateListing(CreateListingViewModel model)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Reload dropdowns in case we need to redisplay the form
            model.Categories = await GetCategoriesAsync();
            model.Conditions = await GetConditionsAsync();

            if (!ModelState.IsValid)
                return View(model);

            var result = await _api.PostAsync<dynamic>("api/listings", new
            {
                title = model.Title,
                description = model.Description,
                price = model.Price,
                categoryId = model.CategoryId,
                conditionId = model.ConditionId,
                listingType = model.ListingType,
                isNegotiable = model.IsNegotiable,
                quantity = model.Quantity,
                campusPickup = model.CampusPickup,
                deliveryAvailable = model.DeliveryAvailable,
            });

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Could not create listing. Make sure your seller profile is active.");
                return View(model);
            }

            return RedirectToAction("Listings");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /seller/offers
        // Received Offers — shows all offers received by the seller
        // ─────────────────────────────────────────────────────────────
        [HttpGet("offers")]
        public async Task<IActionResult> Offers(string filter = "All")
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var offers = await _api.GetAsync<List<ReceivedOfferViewModel>>("api/offers/received");

            var filtered = offers ?? new List<ReceivedOfferViewModel>();
            if (filter != "All")
                filtered = filtered
                    .Where(o => o.Status.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            ViewBag.Offers = filtered;
            ViewBag.ActiveFilter = filter;
            ViewData["SidebarPage"] = "seller-offers";
            return View();
        }

        // ─────────────────────────────────────────────────────────────
        // POST /seller/offers/accept
        // Accepts an offer
        // ─────────────────────────────────────────────────────────────
        [HttpPost("offers/accept")]
        public async Task<IActionResult> AcceptOffer(int offerId)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            await _api.PutAsync($"api/offers/{offerId}/accept");
            return RedirectToAction("Offers");
        }

        // ─────────────────────────────────────────────────────────────
        // POST /seller/offers/reject
        // Rejects an offer
        // ─────────────────────────────────────────────────────────────
        [HttpPost("offers/reject")]
        public async Task<IActionResult> RejectOffer(int offerId)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            await _api.PutAsync($"api/offers/{offerId}/reject");
            return RedirectToAction("Offers");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /seller/activate
        // Activate Seller Profile page
        // ─────────────────────────────────────────────────────────────
        [HttpGet("activate")]
        public IActionResult Activate()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            return View(new ActivateSellerViewModel());
        }

        // ─────────────────────────────────────────────────────────────
        // POST /seller/activate
        // Submits the seller profile activation request
        // ─────────────────────────────────────────────────────────────
        [HttpPost("activate")]
        public async Task<IActionResult> Activate(ActivateSellerViewModel model)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _api.PostAsync<dynamic>("api/sellerprofiles/activate", new
            {
                storeName = model.StoreName,
                sellerBio = model.SellerBio,
                campusPickup = model.CampusPickup,
                deliveryAvailable = model.DeliveryAvailable,
                openToTrades = model.OpenToTrades,
            });

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Could not activate seller profile.");
                return View(model);
            }

            // Update session to reflect seller profile activation
            HttpContext.Session.SetString("HasSellerProfile", "true");

            //Store seller status in a persistent cookie
            //This survives the normal session timeout and allows the UI
            //to keep showing the "Switch to seller" option
            Response.Cookies.Append("HasSellerProfile", "true", new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax
            });
            return RedirectToAction("Index");
        }
        // ─────────────────────────────────────────────────────────────
        // GET /seller/switch
        // Switches the current logged-in buyer into seller mode
        // ─────────────────────────────────────────────────────────────
        [HttpGet("switch")]
        public async Task<IActionResult> SwitchToSeller()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Do not trust the cookie alone.
            // Confirm the seller profile actually exists in the database.
            var profile = await _api.GetAsync<dynamic>("api/sellerprofiles/my");

            if (profile == null)
            {
                HttpContext.Session.Remove("HasSellerProfile");
                Response.Cookies.Delete("HasSellerProfile");

                return RedirectToAction("Activate");
            }

            // Seller profile exists — remember seller capability
            HttpContext.Session.SetString("HasSellerProfile", "true");

            Response.Cookies.Append(
                "HasSellerProfile",
                "true",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });

            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        private async Task<List<CategoryOption>> GetCategoriesAsync()
        {
            var categories = await _api.GetAsync<List<CategoryOption>>("api/categories");
            return categories ?? new List<CategoryOption>();
        }

        private async Task<List<ConditionOption>> GetConditionsAsync()
        {
            var conditions = await _api.GetAsync<List<ConditionOption>>("api/conditions");
            return conditions ?? new List<ConditionOption>();
        }

        // ─────────────────────────────────────────────────────────────
        // POST /seller/listings/delete
        // Soft deletes a listing
        // ─────────────────────────────────────────────────────────────
        [HttpPost("listings/delete")]
        public async Task<IActionResult> DeleteListing(int listingId)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            await _api.DeleteAsync($"api/listings/{listingId}");
            return RedirectToAction("Listings");
        }
    }
}