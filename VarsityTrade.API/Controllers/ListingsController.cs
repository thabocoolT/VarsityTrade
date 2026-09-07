using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase, Route, HttpGet etc
using System.Security.Claims; // Provides ClaimTypes for reading user identity
using VarsityTrade.Core.DTOs.Listings; // Provides Listing DTOs
using VarsityTrade.Core.Interfaces; // Provides IListingService


namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]//All routes start with/api/listings
    public class ListingsController : ControllerBase
    {
        //IListingService is injected-controller stays thin
        private readonly IListingService _listingService;

        public ListingsController(IListingService listingService)
        {
            _listingService= listingService;
        }

        //--------------------------------------------------------
        //GET/api/listings/university/{universityId}
        //Returns all active listings for a specific university
        //Requires authentication-only logged in students can browse
        //-------------------------------------------------
        /// <summary>Returns all active listings for a specific university — campus locked.</summary>
        [HttpGet("University/{university}")]
        [Authorize]//Must be logged in to view listins
        public async Task<IActionResult> GetListingsByUniversity(int universityId)
        {
            var listings=await _listingService.GetListingsByUniversityAsync(universityId);
            return Ok(listings);
        }

        //--------------------------------------------------------
        //GET/api/listings/user/{userId}
        //Returns all active listings for a specific user
        /// <summary>Returns a single listing by ID and increments the view count.</summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult>GetListingById(int id)
            {
            //Increment view count every time the detail page is opened
            await _listingService.IncrementViewCountAsync(id);
            var listing = await _listingService.GetListingByIdAsync(id);

            //Return 404 if the listing does not exist or is deleted
            if(listing==null)
                return NotFound(new {message="Listing not found."});

            return Ok(listing);
        }
        //--------------------------------------------------------
        //POST/api/listings
        //CREATE a new listing-seller only
        //--------------------------------------------------------
        /// <summary>Creates a new listing for the logged in seller.</summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateListing([FromBody] ListingRequestDto request)
        {
            //Read the seller profile iD from the JWT token
            //The seller profile ID must be passed as a claim or looked up
            //For now we read the user ID from the token and look up their profile
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub"); // Fallback for JWT tokens that use "sub" claim
            if(userIdClaim == null)
                return Unauthorized(new {message= "User identity found in token." });

            var userId = int.Parse(userIdClaim.Value);


            //Look up the seller profile ID from the user ID
            //This is done in the service to keep the controller thin
            var result = await _listingService.CreateListingAsync(userId, request);
            if(result==null)
                return BadRequest(new { message = "Could not create listing. Ensure your seller profile is active." });
            
            //Return 201 Created with the new listing ID
            return CreatedAtAction(nameof(GetListingById), new { id = result.ListingId }, result);

            
        }
        //--------------------------------------------------------
        //PUT/api/listings/{id}
        //UPDATE an existing listing-seller only
        /// <summary>Updates an existing listing — seller must own the listing.</summary>

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateListing(int id, [FromBody] ListingRequestDto request)
        {
            //Get the user ID from the JWT token
            var userIdClaim=User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            if(userIdClaim == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var userId = int.Parse(userIdClaim.Value);
            var result=await _listingService.UpdateListingAsync(userId, id, request);
            if(result==null)
                return NotFound(new {message = "Listing not found or you are not authorized to update it." });

            return Ok(result);
        
        }
        //--------------------------------------------------------
        //DELETE/api/listings/{id}
        //Soft delete a listing-seller only
        //--------------------------------------------------------
        /// <summary>Soft deletes a listing — seller must own the listing.</summary>
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteListing(int id)
        {
            //Get the user ID from the JWT token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");

            if(userIdClaim == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var userId = int.Parse(userIdClaim.Value);
            var success = await _listingService.DeleteListingAsync(userId, id);
            if(!success)
                return NotFound(new { message = "Listing not found or you are not authorized to delete it." });

            //Return 204 No Content on successful deletion
            return NoContent();
        }
    }
}
