using Microsoft.AspNetCore.Mvc; // Provides Controller and IActionResult
using VarsityTrade.Web.Models.Listings; // Provides Listing view models
using VarsityTrade.Web.Services; // Provides ApiService

namespace VarsityTrade.Web.Controllers
{
    // HomeController handles the home page and how it works page
    public class HomeController : Controller
    {
        // ApiService handles all HTTP calls to the backend API
        private readonly ApiService _api;

        public HomeController(ApiService api)
        {
            _api = api;
        }

        // GET /
        // Home page — shows featured listings and recent activity
        public async Task<IActionResult> Index()
        {
            // Get the university ID from session
            var universityIdStr = HttpContext.Session.GetString("UniversityId");
            var universityId = int.TryParse(universityIdStr, out var uid) ? uid : 1;

            // Fetch recent listings for the home page feed
            var listings = await _api.GetAsync<List<ListingCardViewModel>>(
                $"api/listings/university/{universityId}");

            // Pass the listings to the view — take only the first 6 for the home page
            ViewBag.RecentListings = listings?.Take(6).ToList() ?? new List<ListingCardViewModel>();
            ViewBag.FeaturedListings = listings?.Where(l => l.IsFeatured).Take(3).ToList()
                ?? new List<ListingCardViewModel>();

            return View();
        }
        [HttpGet("home/how-it-works")]
        public IActionResult HowItWorks()
        {
            ViewData["Title"] = "How It Works";
            ViewData["InfoPage"] = "How It Works";

            return View("Info");
        }

        [HttpGet("home/about")]
        public IActionResult About()
        {
            ViewData["Title"] = "About Varsity Trade";
            ViewData["InfoPage"] = "About Varsity Trade";

            return View("Info");
        }

        [HttpGet("home/terms")]
        public IActionResult Terms()
        {
            ViewData["Title"] = "Terms of Service";
            ViewData["InfoPage"] = "Terms of Service";

            return View("Info");
        }

        [HttpGet("home/privacy")]
        public IActionResult Privacy()
        {
            ViewData["Title"] = "Privacy Policy";
            ViewData["InfoPage"] = "Privacy Policy";

            return View("Info");
        }

        [HttpGet("home/contact")]
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact";
            ViewData["InfoPage"] = "Contact";

            return View("Info");
        }
    }
}