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

        /// <summary>Returns all categories including subcategories.</summary>
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.ParentCategoryId)
                .ThenBy(c => c.Name)
                .Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.IconName,
                    c.ParentCategoryId
                })
                .ToListAsync();

            return Ok(categories);
        }
    }
}