using Microsoft.AspNetCore.Mvc;
using VarsityTrade.Web.Models.Home;
using VarsityTrade.Web.Models.Listings;
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
            var isLoggedIn =
                !string.IsNullOrWhiteSpace(
                    HttpContext.Session.GetString("AccessToken"));

            List<ListingCardViewModel> listings;

            if (isLoggedIn)
            {
                var universityIdStr =
                    HttpContext.Session.GetString("UniversityId");

                if (int.TryParse(universityIdStr, out var universityId))
                {
                    listings =
                        await _api.GetAsync<List<ListingCardViewModel>>(
                            $"api/listings/university/{universityId}")
                        ?? new List<ListingCardViewModel>();
                }
                else
                {
                    listings = new List<ListingCardViewModel>();
                }
            }
            else
            {
                listings =
                    await _api.GetAsync<List<ListingCardViewModel>>(
                        "api/listings/public")
                    ?? new List<ListingCardViewModel>();
            }

            ViewBag.RecentListings =
                listings.Take(6).ToList();

            ViewBag.FeaturedListings =
                listings
                    .Where(l => l.IsFeatured)
                    .Take(3)
                    .ToList();

            // Load real platform statistics
            var stats =
                await _api.GetAsync<PublicHomeStatsViewModel>(
                    "api/admin/public-stats")
                ?? new PublicHomeStatsViewModel();

            ViewBag.ActiveStudents = stats.ActiveStudents;
            ViewBag.ItemsListed = stats.ItemsListed;
            ViewBag.TradesDone = stats.TradesDone;
            ViewBag.VerificationRate = stats.VerificationRate;

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