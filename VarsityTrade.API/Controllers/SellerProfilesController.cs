using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using System.Security.Claims; // Provides ClaimTypes for reading JWT claims
using VarsityTrade.Core.DTOs.SellerProfiles; // Provides SellerProfile DTOs
using VarsityTrade.Core.Interfaces; // Provides ISellerProfileService

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]//all routes start with /api/sellerprofiles
    public class SellerProfilesController : ControllerBase
    {
        //ISellerProfileService is injected-controller stays thin
        private readonly ISellerProfileService _sellerProfileService;
        public SellerProfilesController(ISellerProfileService sellerProfileService)
        {
            _sellerProfileService = sellerProfileService;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/sellerprofiles/{id}
        // Returns a seller profile by ID — for the public profile page
        /// <summary>Returns a seller profile by ID — for the public profile page.</summary>
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        [Authorize] 
        public async Task<IActionResult> GetSellerProfileById(int id)
        {
            var profile =await _sellerProfileService.GetSellerProfileByIdAsync(id);
            if(profile== null) 
                return NotFound(new { message = "Seller profile not found." });
            return Ok(profile);
        
        }

        //-----------------------------------------------------------
        // GET /api/sellerprofiles/me
        //Return the logged in seller's own profile-for the seller dashboard
        //-----------------------------------------------------------
        /// <summary>Returns the logged in seller's own profile.</summary>

        [HttpGet("my")]
        [Authorize]
        public async Task<IActionResult> GetMySellerProfile()
        {
            //Get the user ID from the JWT claims
            var userIdClaim=User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub"); // fallback for some JWTs

            if(userIdClaim == null)
                return Unauthorized(new { message = "User identity not found in token" });

            var userId = int.Parse(userIdClaim.Value);
            var profile=await _sellerProfileService.GetSellerProfileByUserIdAsync(userId);
            
            if(profile == null)
                return NotFound(new { message = "You do not have an active seller profile" });
            return Ok(profile);

        }
        // ─────────────────────────────────────────────────────────────
        //POST /api/sellerprofiles/activate
        //Activate a seller profile for the logged in user
        //A buyer becomes a seller by activating a seller profile
        // ─────────────────────────────────────────────────────────────
        /// <summary>Activates a seller profile for the logged in user — one time setup.</summary>
        [HttpPost("activate")]
        [Authorize]
        public async Task<IActionResult> ActivateSellerProfile([FromBody] SellerProfileRequestDto request)
        {
            //GET the user ID from the JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub"); // fallback for some JWTs

            if(userIdClaim == null)
                return Unauthorized(new { message = "User identity not found in token" });

            var userId = int.Parse(userIdClaim.Value);
            var result = await _sellerProfileService.ActivateSellerProfileAsync(userId, request);
            if(result==null)
                return BadRequest(new {message = "Seller profile already exists or could not be created." });

            //Return 201 Created with the new seller profile
            return CreatedAtAction(nameof(GetMySellerProfile), new { id = result.SellerProfileId }, result);

        }
        // ─────────────────────────────────────────────────────────────
        //PUT /api/sellerprofiles/me
        //Update the logged in seller's own profile
        // ─────────────────────────────────────────────────────────────
        /// <summary>Updates the logged in seller's profile settings.</summary>
        [HttpPut("my")]
        [Authorize]
        public async Task<IActionResult>UpdateMySellerProfile([FromBody] SellerProfileRequestDto request)
        {
            //GET the user ID from the JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub"); // fallback for some JWTs

            if(userIdClaim == null)
                return Unauthorized(new { message = "User identity not found in token" });
            var userId=int.Parse(userIdClaim.Value);
            var result=await _sellerProfileService.UpdateSellerProfileAsync(userId, request);
            if(result== null)
                return NotFound(new { message = "Seller profile not found or could not be updated." });

            return Ok(result);
        }

    }
}
