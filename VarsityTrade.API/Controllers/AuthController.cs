using Microsoft.AspNetCore.Mvc; // Provides ControllerBase, Route, HttpPost etc
using VarsityTrade.Core.DTOs.Auth; // Provides Auth DTOs
using VarsityTrade.Core.Interfaces; // Provides IAuthService

namespace VarsityTrade.API.Controllers
{
    // ApiController attribute enables automatic model validation
    // and returns 400 Bad Request if the DTO validation fails
    [ApiController]

    // All routes in this controller start with /api/auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // IAuthService is injected — the controller never knows about AuthService directly
        // This keeps the controller thin and the business logic in the service
        private readonly IAuthService _authService;

        // Constructor receives IAuthService via dependency injection
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/register
        // Registers a new student account
        // ─────────────────────────────────────────────────────────────
        /// <summary>Registers a new student account and returns a JWT token pair.</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            // Call the auth service to handle registration logic
            var result = await _authService.RegisterAsync(request);

            // If result is null the email already exists or registration failed
            if (result == null)
                return BadRequest(new { message = "Registration failed. Email may already be in use." });

            // Return 200 OK with the token and user info
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/login
        // Logs in an existing student and returns JWT tokens
        // ─────────────────────────────────────────────────────────────
        /// <summary>Logs in an existing student and returns a JWT token pair.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            // Call the auth service to verify credentials and generate tokens
            var result = await _authService.LoginAsync(request);

            // If result is null the credentials were invalid
            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            // Return 200 OK with the token and user info
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/refresh
        // Generates a new access token using a valid refresh token
        // ─────────────────────────────────────────────────────────────
        /// <summary>Generates a new access token using a valid refresh token.</summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] string refreshToken)
        {
            // Call the auth service to validate and rotate the refresh token
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // If result is null the refresh token is invalid or expired
            if (result == null)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            // Return 200 OK with the new token pair
            return Ok(result);
        }
    }
}