using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult

using VarsityTrade.Web.Models.Buyer; // Provides Buyer view models
using VarsityTrade.Web.Models.Listings; // Provides ListingCardViewModel
using VarsityTrade.Web.Services; // Provides ApiService

namespace VarsityTrade.Web.Controllers
{
    // BuyerController handles all buyer dashboard pages
    // Requires the user to be logged in — checks session token
    [Route("buyer")]
    public class BuyerController : Controller
    {
        // ApiService handles all HTTP calls to the backend API
        private readonly ApiService _api;

        public BuyerController(ApiService api)
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

        // Helper — gets the current user ID from session
        private int GetUserId() =>
            int.TryParse(HttpContext.Session.GetString("UserId"), out var id) ? id : 0;

        // ─────────────────────────────────────────────────────────────
        // GET /buyer
        // Buyer dashboard — shows stats and recent activity
        // ─────────────────────────────────────────────────────────────
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Build the dashboard view model
            var model = new BuyerDashboardViewModel
            {
                FirstName = HttpContext.Session.GetString("FirstName") ?? string.Empty,
                LastName = HttpContext.Session.GetString("LastName") ?? string.Empty,
                UniversityName = "Your University",
            };

            // Load recent offers for the dashboard preview
            var offers = await _api.GetAsync<List<OfferViewModel>>("api/offers/my");
            if (offers != null)
            {
                model.ActiveOffersCount = offers.Count(o => o.Status == "Pending");
                model.RecentOffers = offers.Take(3).Select(o => new OfferSummaryViewModel
                {
                    OfferId = o.OfferId,
                    ListingTitle = o.ListingTitle,
                    OfferAmount = o.OfferAmount,
                    OfferType = o.OfferType,
                    Status = o.Status,
                    CreatedAt = o.CreatedAt,
                }).ToList();
            }

            // Load unread notification count
            var unreadResult = await _api.GetAsync<UnreadCountResult>("api/notifications/unread-count");
            model.UnreadNotificationsCount = unreadResult?.UnreadCount ?? 0;

            // Load conversations for unread message count
            var conversations = await _api.GetAsync<List<ConversationViewModel>>("api/messaging/conversations");
            model.UnreadMessagesCount = conversations?.Sum(c => c.UnreadCount) ?? 0;

            ViewData["SidebarPage"] = "buyer-dashboard";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /buyer/inbox
        // Inbox — shows all conversations
        // ─────────────────────────────────────────────────────────────
        [HttpGet("inbox")]
        public async Task<IActionResult> Inbox()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var conversations = await _api.GetAsync<List<ConversationViewModel>>("api/messaging/conversations")
                                ?? new List<ConversationViewModel>();

            var model = new InboxViewModel
            {
                Conversations = conversations,
                UnreadCount = conversations.Sum(c => c.UnreadCount),
            };

            // Use seller inbox view when in seller mode
            var currentMode = HttpContext.Session.GetString("CurrentMode") ?? "Buyer";
            ViewData["SidebarPage"] = currentMode == "Seller" ? "seller-inbox" : "inbox";

            return currentMode == "Seller"
                ? View("~/Views/Seller/Inbox.cshtml", model)
                : View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /buyer/inbox/{id}
        // Conversation thread — shows messages in a conversation
        // ─────────────────────────────────────────────────────────────
        [HttpGet("inbox/{id:int}")]
        public async Task<IActionResult> Conversation(int id)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Load the conversation details
            var conversation = await _api.GetAsync<ConversationViewModel>(
                $"api/messaging/conversations/{id}");

            if (conversation == null)
                return NotFound();

            // Load all messages in the conversation — also marks them as read
            var messages = await _api.GetAsync<List<MessageViewModel>>(
                $"api/messaging/conversations/{id}/messages");

            var model = new ConversationThreadViewModel
            {
                Conversation = conversation,
                Messages = messages ?? new List<MessageViewModel>(),
                CurrentUserId = GetUserId(),
            };

            ViewData["SidebarPage"] = "inbox";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /buyer/offers
        // My Offers — shows all offers made by the buyer
        // ─────────────────────────────────────────────────────────────
        [HttpGet("offers")]
        public async Task<IActionResult> Offers(string filter = "All")
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var offers = await _api.GetAsync<List<OfferViewModel>>("api/offers/my");

            var model = new OffersViewModel
            {
                ActiveFilter = filter,
                Offers = offers ?? new List<OfferViewModel>(),
            };

            // Apply filter
            if (filter != "All")
                model.Offers = model.Offers
                    .Where(o => o.Status.Equals(filter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            ViewData["SidebarPage"] = "offers";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /buyer/notifications
        // Notifications page
        // ─────────────────────────────────────────────────────────────
        [HttpGet("notifications")]
        public async Task<IActionResult> Notifications()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var notifications = await _api.GetAsync<List<NotificationViewModel>>(
                "api/notifications");

            var unreadResult = await _api.GetAsync<UnreadCountResult>(
                "api/notifications/unread-count");

            var model = new NotificationsViewModel
            {
                Notifications = notifications ?? new List<NotificationViewModel>(),
                UnreadCount = unreadResult?.UnreadCount ?? 0,
            };

            ViewData["SidebarPage"] = "notifications";
            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /buyer/profile
        // Profile page
        // ─────────────────────────────────────────────────────────────
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            ViewData["SidebarPage"] = "profile";
            return View();
        }

        //-----------------------------------------------------------
        //Buyer saved listings
        //-----------------------------------------------------------
        [HttpGet("saved")]
        public async Task<IActionResult> Saved(string filter = "All")
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            // Deferred to QA — saved listings API endpoint
            // For now show empty state
            ViewBag.SavedListings = new List<VarsityTrade.Web.Models.Listings.ListingCardViewModel>();
            ViewBag.ActiveFilter = filter;
            ViewData["SidebarPage"] = "saved";
            return View();
        }

        //-------------------------------------------------────────----
        //Buyer reviews
        //-------------------------------------------------────────----
        [HttpGet("reviews")]
        public async Task<IActionResult> Reviews(string filter = "Reviews left")
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var reviews = await _api.GetAsync<List<VarsityTrade.Web.Models.Reviews.ReviewViewModel>>(
                "api/reviews/my") ?? new List<VarsityTrade.Web.Models.Reviews.ReviewViewModel>();

            ViewBag.Reviews = reviews;
            ViewBag.ActiveFilter = filter;
            ViewData["SidebarPage"] = "reviews";
            return View();
        }
        // ─────────────────────────────────────────────────────────────
        // POST /buyer/inbox/send
        // Sends a message in a conversation
        // ─────────────────────────────────────────────────────────────
        [HttpPost("inbox/send")]
        public async Task<IActionResult> SendMessage(int conversationId, string content)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            await _api.PostAsync<object>($"api/messaging/conversations/{conversationId}/messages", new
            {
                content = content,
                messageType = "Text"
            });

            return RedirectToAction("Conversation", new { id = conversationId });
        }

        // ─────────────────────────────────────────────────────────────
        // POST /buyer/offers/cancel
        // Cancels a pending offer
        // ─────────────────────────────────────────────────────────────
        [HttpPost("offers/cancel")]
        public async Task<IActionResult> CancelOffer(int offerId)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            await _api.PutAsync($"api/offers/{offerId}/cancel");
            return RedirectToAction("Offers");
        }

        // ─────────────────────────────────────────────────────────────
        // POST /buyer/notifications/read-all
        // Marks all notifications as read
        // ─────────────────────────────────────────────────────────────
        [HttpPost("notifications/read-all")]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            await _api.PutAsync("api/notifications/read-all");
            return RedirectToAction("Notifications");
        }

        [HttpGet("offers/make")]
        public async Task<IActionResult> MakeOffer(int listingId)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var listing = await _api.GetAsync<VarsityTrade.Web.Models.Listings.ListingDetailViewModel>(
                $"api/listings/{listingId}");

            if (listing == null)
                return NotFound();

            ViewBag.Listing = listing;
            ViewData["SidebarPage"] = "offers";
            return View();
        }

        [HttpPost("offers/make")]
        public async Task<IActionResult> MakeOffer(int listingId, string offerType,
            decimal? offerAmount, string? tradeItem1, decimal? tradeItem1Value,
            string? tradeItem2, decimal? tradeItem2Value)
        {
            var auth = RequireAuth();
            if (auth != null) return auth;

            var offerItems = new List<object>();
            if (!string.IsNullOrEmpty(tradeItem1))
                offerItems.Add(new { title = tradeItem1, estimatedValue = tradeItem1Value });
            if (!string.IsNullOrEmpty(tradeItem2))
                offerItems.Add(new { title = tradeItem2, estimatedValue = tradeItem2Value });

            var result = await _api.PostAsync<dynamic>("api/offers", new
            {
                listingId = listingId,
                offerType = offerType,
                offerAmount = offerAmount,
                offerItems = offerItems,
            });

            if (result == null)
            {
                TempData["Error"] = "Could not submit offer. Please try again.";
                return RedirectToAction("MakeOffer", new { listingId });
            }

            TempData["Success"] = "Offer submitted successfully!";
            return RedirectToAction("Offers");
        }
    }

    // Helper class for deserializing the unread count response
    public class UnreadCountResult
    {
        public int UnreadCount { get; set; }
    }

    
}