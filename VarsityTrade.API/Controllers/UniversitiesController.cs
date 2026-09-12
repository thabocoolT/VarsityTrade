using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using Microsoft.EntityFrameworkCore; // Provides ToListAsync
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // All routes start with /api/universities
    public class UniversitiesController : ControllerBase
    {
        // DbContext injected for direct database access
        private readonly VarsityTradeDbContext _context;

        public UniversitiesController(VarsityTradeDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all active universities for the registration dropdown. No authentication required.</summary>
        [HttpGet]
        public async Task<IActionResult> GetUniversities()
        {
            // Load all active universities ordered by name
            // This endpoint is public — guests need it to register
            var universities = await _context.Universities
                .Where(u => u.IsActive)              // Only active universities
                .OrderBy(u => u.Name)                // Alphabetical order
                .Select(u => new                     // Return only the fields needed
                {
                    u.UniversityId,
                    u.Name,
                    u.ShortName,
                    u.City,
                    u.Province
                })
                .ToListAsync();

            return Ok(universities);
        }
    }
} 