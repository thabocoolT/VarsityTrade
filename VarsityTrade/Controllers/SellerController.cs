using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using System.Text.Json; // Provides JsonElement for safe API response mapping
using VarsityTrade.Web.Models.Seller; // Provides Seller view models
using VarsityTrade.Web.Services; // Provides ApiService
using Newtonsoft.Json;

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
            if(string.IsNullOrWhiteSpace(
                HttpContext.Session.GetString("AccessToken")))
            {
                return RedirectToAction("Login", "Auth");
            }
            return null;
        }
        private IActionResult? RequireSellerMode()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var currentMode =
                HttpContext.Session.GetString("CurrentMode");

            var hasSellerProfile =
                HttpContext.Session.GetString("HasSellerProfile") == "true";

            if(currentMode != "Seller" || !hasSellerProfile)
            {
                return RedirectToAction("SwitchToSeller");
            }
            return null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /seller
        // Seller dashboard — shows stats, recent listings, pending offers
        // ─────────────────────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var auth = RequireSellerMode();
            if (auth != null) return auth;

            // Load the seller profile
            var profile = await _api.GetAsync<dynamic>("api/sellerprofiles/my");
            if (profile == null)
                return RedirectToAction("Activate");

            // Load listings
            var listings = await _api.GetAsync<List<SellerListingViewModel>>("api/listings/my");

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
                ActiveListingsCount =
                listings?.Count(l =>
                    string.Equals(
                        l.Status,
                        "Active",
                        StringComparison.OrdinalIgnoreCase)) ?? 0,
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
        public async Task<IActionResult> Listings(
    string filter = "All")
        {
            var auth = RequireSellerMode();

            if (auth != null)
                return auth;

            var listings =
                await _api.GetAsync<
                    List<SellerListingViewModel>>(
                        "api/listings/my")
                ?? new List<SellerListingViewModel>();

            var filtered =
                filter.Equals(
                    "All",
                    StringComparison.OrdinalIgnoreCase)
                    ? listings
                    : listings
                        .Where(l =>
                            l.Status.Equals(
                                filter,
                                StringComparison.OrdinalIgnoreCase))
                        .ToList();

            ViewBag.Listings = filtered;
            ViewBag.ActiveFilter = filter;

            ViewData["SidebarPage"] =
                "seller-listings";

            return View();
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

            var allCategories = await GetCategoriesAsync();
            var conditions = await GetConditionsAsync();

            var model = new CreateListingViewModel
            {
                Categories = allCategories,
                Conditions = conditions,
            };

            ViewData["SidebarPage"] = "seller-create";
            return View(model);
        }



        // ─────────────────────────────────────────────────────────────
        // POST /seller/listings/create
        // Submits a new listing to the API
        // ─────────────────────────────────────────────────────────────
        [HttpPost("listings/create")]
        public async Task<IActionResult> CreateListing(CreateListingViewModel model, string? imageUrls)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

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
            var auth = RequireSellerMode();
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
            var auth = RequireSellerMode();
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
            var auth = RequireSellerMode();
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

            // Mark seller profile as active in session
            HttpContext.Session.SetString("HasSellerProfile", "true");

            // Store store name in session for navbar display
            HttpContext.Session.SetString("StoreName", model.StoreName);

            // Switch to seller mode immediately after activation
            HttpContext.Session.SetString("CurrentMode", "Seller");

            return RedirectToAction("Index");
        }
        // ─────────────────────────────────────────────────────────────
        // GET /seller/switch-to-seller
        // Switches the current session mode to Seller
        // ─────────────────────────────────────────────────────────────
        
        [HttpGet("switch-to-seller")]
        public async Task<IActionResult> SwitchToSeller()
        {
            var auth = RequireAuth();

            if (auth != null)
                return auth;

            var profile =
                await _api.GetAsync<dynamic>("api/sellerprofiles/my");

            if (profile == null)
            {
                HttpContext.Session.SetString(
                    "HasSellerProfile",
                    "false");

                HttpContext.Session.SetString(
                    "CurrentMode",
                    "Buyer");

                return RedirectToAction("Activate");
            }

            HttpContext.Session.SetString(
                "HasSellerProfile",
                "true");

            HttpContext.Session.SetString(
                "CurrentMode",
                "Seller");

            return RedirectToAction("Index");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /seller/switch-to-buyer
        // Switches the current session mode to Buyer
        // ─────────────────────────────────────────────────────────────

        [HttpGet("switch-to-buyer")]
        public IActionResult SwitchToBuyer()
        {
            var auth = RequireAuth();

            if (auth != null)
                return auth;

            HttpContext.Session.SetString("CurrentMode", "Buyer");

            return RedirectToAction("Index", "Home");
        }

        //----------------------------------------------------------------
        //GET/seller/reviews
        //Shows all reviews received by the seller
        [HttpGet("reviews")]
        public async Task<IActionResult> Reviews(string filter = "All")
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Get seller profile ID from session or API
            var profile = await _api.GetAsync<dynamic>("api/sellerprofiles/my");
            int sellerProfileId = 0;
            if (profile != null)
                sellerProfileId = (int)(profile.sellerProfileId ?? 0);

            var reviews = sellerProfileId > 0
                ? await _api.GetAsync<List<VarsityTrade.Web.Models.Reviews.ReviewViewModel>>(
                    $"api/reviews/seller/{sellerProfileId}") ?? new()
                : new List<VarsityTrade.Web.Models.Reviews.ReviewViewModel>();

            var filtered = filter switch
            {
                "5 stars" => reviews.Where(r => r.Rating == 5).ToList(),
                "4 stars" => reviews.Where(r => r.Rating == 4).ToList(),
                "1-3 stars" => reviews.Where(r => r.Rating <= 3).ToList(),
                _ => reviews
            };

            ViewBag.Reviews = filtered;
            ViewBag.ActiveFilter = filter;
            ViewData["SidebarPage"] = "seller-reviews";
            return View();
        }
        // ─────────────────────────────────────────────────────────────
        // GET /seller/listings/edit/{id}
        // Edit an existing listing
        // ─────────────────────────────────────────────────────────────
        [HttpGet("listings/edit/{id:int}")]
        public async Task<IActionResult> EditListing(int id)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Fetch the existing listing
            var listing = await _api.GetAsync<dynamic>($"api/listings/{id}");
            if (listing == null) return NotFound();

            var allCategories = await GetCategoriesAsync();
            var conditions = await GetConditionsAsync();

            // Pre-populate the view model with existing data
            var model = new EditListingViewModel
            {
                ListingId = id,
                Title = (string)(listing.title ?? ""),
                Description = (string)(listing.description ?? ""),
                Price = (decimal)(listing.price ?? 0),
                CategoryId = (int)(listing.categoryId ?? 0),
                ConditionId = (int)(listing.conditionId ?? 0),
                ListingType = (string)(listing.listingType ?? "Sale"),
                IsNegotiable = (bool)(listing.isNegotiable ?? false),
                Quantity = (int)(listing.quantity ?? 1),
                CampusPickup = (bool)(listing.campusPickup ?? true),
                DeliveryAvailable = (bool)(listing.deliveryAvailable ?? false),
                CurrentStatus = (string)(listing.status ?? ""),
                Categories = allCategories,
                Conditions = conditions,
            };

            ViewData["SidebarPage"] = "seller-listings";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /seller/listings/edit/{id}
        // Saves edits to an existing listing
        // ─────────────────────────────────────────────────────────────
        [HttpPost("listings/edit/{id:int}")]
        public async Task<IActionResult> EditListing(int id, EditListingViewModel model, string? imageUrls)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            model.Categories = await GetCategoriesAsync();
            model.Conditions = await GetConditionsAsync();
            model.ListingId = id;

            if (!ModelState.IsValid)
                return View(model);

            var result = await _api.PutAsync<dynamic>($"api/listings/{id}", new
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
                ModelState.AddModelError(string.Empty, "Could not update listing. Please try again.");
                return View(model);
            }

            TempData["Success"] = "Listing updated successfully.";
            return RedirectToAction("Listings");
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
            var auth = RequireSellerMode();
            if (auth != null) return auth;

            await _api.DeleteAsync($"api/listings/{listingId}");
            return RedirectToAction("Listings");
        }

        [HttpPost("listings/mark-sold")]
        public async Task<IActionResult> MarkAsSold(int listingId)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            await _api.PutAsync($"api/listings/{listingId}/sold");
            return RedirectToAction("Listings");
        }

    }
}