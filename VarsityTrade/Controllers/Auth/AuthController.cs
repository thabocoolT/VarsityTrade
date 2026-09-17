using Microsoft.AspNetCore.Mvc; // Provides Controller, IActionResult
using VarsityTrade.Web.Models.Auth; // Provides Auth view models
using VarsityTrade.Web.Services; // Provides ApiService

namespace VarsityTrade.Web.Controllers.Auth
{
    // AuthController handles all authentication pages
    // Register, Login, and Logout
    [Route("auth")]
    public class AuthController : Controller
    {
        // ApiService handles all HTTP calls to the backend API
        private readonly ApiService _api;

        // Constructor receives ApiService via dependency injection
        public AuthController(ApiService api)
        {
            _api = api;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /auth/login
        // Shows the login page
        // ─────────────────────────────────────────────────────────────
        [HttpGet("login")]
        public IActionResult Login()
        {
            // If already logged in redirect to home
            if (HttpContext.Session.GetString("AccessToken") != null)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel());
        }

        // ─────────────────────────────────────────────────────────────
        // POST /auth/login
        // Submits login credentials to the API and stores the token
        // ─────────────────────────────────────────────────────────────
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Validate the form — return with errors if invalid
            if (!ModelState.IsValid)
                return View(model);

            // Call the API login endpoint
            var result = await _api.PostAsync<AuthResponseViewModel>("api/auth/login", new
            {
                email = model.Email,
                password = model.Password
            });

            // If login failed show an error message
            if (result == null)
            {
                model.ErrorMessage = "Invalid email or password. Please try again.";
                return View(model);
            }

            // Store the token and user info in session
            StoreUserSession(result);

            // Check if this user already has a seller profile and store it in session
            await CheckAndStoreSellerProfileAsync(result.AccessToken);

            // Redirect based on role
            if (result.Role == "Admin")
                return RedirectToAction("Index", "Admin", new { area = "" });

            return RedirectToAction("Index", "Home");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /auth/register
        // Shows the registration page with university dropdown
        // ─────────────────────────────────────────────────────────────
        [HttpGet("register")]
        public async Task<IActionResult> Register()
        {
            // If already logged in redirect to home
            if (HttpContext.Session.GetString("AccessToken") != null)
                return RedirectToAction("Index", "Home");

            // Load universities for the dropdown from the API
            var model = new RegisterViewModel
            {
                Universities = await GetUniversitiesAsync()
            };

            return View(model);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /auth/register
        // Submits registration data to the API and stores the token
        // ─────────────────────────────────────────────────────────────
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Reload universities in case we need to redisplay the form
            model.Universities = await GetUniversitiesAsync();

            // Validate the form
            if (!ModelState.IsValid)
                return View(model);

            // Call the API register endpoint
            var result = await _api.PostAsync<AuthResponseViewModel>("api/auth/register", new
            {
                firstName = model.FirstName,
                lastName = model.LastName,
                email = model.Email,
                password = model.Password,
                universityId = model.UniversityId,
                suburb = model.Suburb,
                city = model.City,
                province = model.Province,
                residenceName = model.ResidenceName,
                studentNumber = model.StudentNumber
            });

            // If registration failed show an error
            if (result == null)
            {
                ModelState.AddModelError(string.Empty,
                    "Registration failed. This email may already be registered.");
                return View(model);
            }

            // Store the token and user info in session
            StoreUserSession(result);

            // New users always start as Buyer — redirect to home
            return RedirectToAction("Index", "Home");
        }

        // ─────────────────────────────────────────────────────────────
        // GET /auth/logout
        // Clears the session and redirects to home
        // ─────────────────────────────────────────────────────────────
        [HttpGet("logout")]
        public IActionResult Logout()
        {
            // Clear all session data — removes the JWT token and user info
            HttpContext.Session.Clear();

            // Redirect to the home page
            return RedirectToAction("Index", "Home");
        }

        // ─────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────

        // Stores all user data in session after successful login or register
        // Session is used across all controllers to identify the current user
        private void StoreUserSession(AuthResponseViewModel result)
        {
            HttpContext.Session.SetString("AccessToken", result.AccessToken);
            HttpContext.Session.SetString("RefreshToken", result.RefreshToken);
            HttpContext.Session.SetString("UserId", result.UserId.ToString());
            HttpContext.Session.SetString("UserEmail", result.Email);
            HttpContext.Session.SetString("UserRole", result.Role);
            HttpContext.Session.SetString("UniversityId", result.UniversityId.ToString());
            HttpContext.Session.SetString("FirstName", result.FirstName);
            HttpContext.Session.SetString("LastName", result.LastName);

            // Store initials for the avatar circle in the navbar
            var initials = $"{result.FirstName[0]}{result.LastName[0]}".ToUpper();
            HttpContext.Session.SetString("UserInitials", initials);

            // HasSellerProfile defaults to false — will be updated after checking the API
            HttpContext.Session.SetString("HasSellerProfile", "false");

            // Default mode is Buyer — user switches to Seller explicitly
            // This controls which sidebar and features are shown
            HttpContext.Session.SetString("CurrentMode", "Buyer");
        }

        // Loads the list of universities from the API for the register dropdown
        private async Task<List<UniversityOption>> GetUniversitiesAsync()
        {
            // The universities endpoint is public — no auth required
            var universities = await _api.GetAsync<List<UniversityOption>>("api/universities");
            return universities ?? new List<UniversityOption>();
        }
        // Checks if the logged in user has an active seller profile
        // and stores the result in session so the sidebar and navbar show correctly
        private async Task CheckAndStoreSellerProfileAsync(string accessToken)
        {
            try
            {
                // Set the token in session so ApiService can attach it
                HttpContext.Session.SetString("AccessToken", accessToken);

                // Use GetWithStatusAsync so we can distinguish 200 vs 404 vs 401
                var (success, _, statusCode) = await _api.GetWithStatusAsync<SellerProfileExistsViewModel>(
                    "api/sellerprofiles/my");



                if (success && statusCode == 200)
                {
                    // User has an active seller profile
                    HttpContext.Session.SetString("HasSellerProfile", "true");
                }
                else
                {
                    // 404 = no seller profile yet, 401 = token issue
                    HttpContext.Session.SetString("HasSellerProfile", "false");
                }

                // Always start in Buyer mode on login
                HttpContext.Session.SetString("CurrentMode", "Buyer");
            }
            catch
            {
                HttpContext.Session.SetString("HasSellerProfile", "false");
                HttpContext.Session.SetString("CurrentMode", "Buyer");
            }
        }

        [HttpGet("forgot-password")]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword(string email)
        {
            // Deferred to QA phase — email service not yet implemented
            // Show success message for now
            TempData["Message"] = "If that email is registered, a reset link has been sent.";
            return RedirectToAction("Login");
        }
    }
}