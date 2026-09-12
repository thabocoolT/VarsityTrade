using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using Microsoft.EntityFrameworkCore; // Provides ToListAsync
using VarsityTrade.Infrastructure.Data; // Provides VarsityTradeDbContext

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConditionsController : ControllerBase
    {
        private readonly VarsityTradeDbContext _context;

        public ConditionsController(VarsityTradeDbContext context)
        {
            _context = context;
        }

        /// <summary>Returns all item conditions for the listing form dropdown.</summary>
        [HttpGet]
        public async Task<IActionResult> GetConditions()
        {
            var conditions = await _context.Condition
                .OrderBy(c => c.ConditionId)
                .Select(c => new { c.ConditionId, c.Name })
                .ToListAsync();

            return Ok(conditions);
        }
    }
}