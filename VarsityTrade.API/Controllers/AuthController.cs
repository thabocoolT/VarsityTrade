
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using VarsityTrade.Core.DTOs.Auth;
using VarsityTrade.Core.Interfaces;

namespace VarsityTrade.API.Controllers
{
    // ApiController attribute enables automatic model validation
    // and returns 400 Bad Request if the DTO validation fails
    [ApiController]

    // All routes in this controller start with /api/auth
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        // IAuthService is injected through dependency injection
        // The controller depends on the interface, not the concrete service
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
        /// <summary>
        /// Registers a new student account and returns a JWT token pair.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterRequestDto request)
        {
            // Call the authentication service
            var result = await _authService.RegisterAsync(request);

            // Registration failed
            if (result == null)
            {
                return BadRequest(new
                {
                    message = "Registration failed. Email may already be in use."
                });
            }

            // Return the authentication response
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/login
        // Logs in an existing student
        // ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Logs in an existing student and returns a JWT token pair.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request)
        {
            // Call the authentication service
            var result = await _authService.LoginAsync(request);

            // Login failed
            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });
            }

            // Return the authentication response
            return Ok(result);
        }

        // ─────────────────────────────────────────────────────────────
        // POST /api/auth/refresh
        // Generates a new access token using a valid refresh token
        // ─────────────────────────────────────────────────────────────
        /// <summary>
        /// Generates a new access token using a valid refresh token.
        /// </summary>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] string refreshToken)
        {
            // Call the authentication service to validate
            // and rotate the refresh token
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // Refresh token is invalid or expired
            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid or expired refresh token."
                });
            }

            // Return the new token pair
            return Ok(result);
        }
    }
}
