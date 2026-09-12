using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using Microsoft.EntityFrameworkCore; // Provides ToListAsync
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly VarsityTradeDbContext _context;

        public CategoriesController(VarsityTradeDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all categories for the listing form dropdown.</summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            // Return parent categories only for the main dropdown
            var categories = await _context.Categories
                .Where(c => c.ParentCategoryId == null) // Parent categories only
                .OrderBy(c => c.Name)
                .Select(c => new { c.CategoryId, c.Name, c.IconName })
                .ToListAsync();

            return Ok(categories);
        }
    }
}