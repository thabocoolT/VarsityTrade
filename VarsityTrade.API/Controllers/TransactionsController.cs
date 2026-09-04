

using Microsoft.AspNetCore.Authorization; // Provides Authorize attribute
using Microsoft.AspNetCore.Mvc; // Provides ControllerBase and action results
using System.Security.Claims; // Provides ClaimTypes for reading JWT claims
using VarsityTrade.Core.Interfaces; // Provides ITransactionService

namespace VarsityTrade.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // All routes start with /api/transactions
    [Authorize] // All transaction endpoints require authentication
    public class TransactionsController : ControllerBase
    {
        // ITransactionService injected — controller stays thin
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        // Helper to read user ID from JWT token
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub");
            return claim != null ? int.Parse(claim.Value) : null;
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/transactions/purchases
        // Returns all transactions where the user is the buyer
        // Used on the buyer's purchase history page
        // ─────────────────────────────────────────────────────────────
        [HttpGet("purchases")]
        public async Task<IActionResult> GetMyPurchases()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var transactions = await _transactionService.GetTransactionsByBuyerAsync(userId.Value);
            return Ok(transactions);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/transactions/sales
        // Returns all transactions where the user is the seller
        // Used on the seller's sales history page
        // ─────────────────────────────────────────────────────────────
        [HttpGet("sales")]
        public async Task<IActionResult> GetMySales()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var transactions = await _transactionService.GetTransactionsBySellerAsync(userId.Value);
            return Ok(transactions);
        }

        // ─────────────────────────────────────────────────────────────
        // GET /api/transactions/{id}
        // Returns a single transaction — user must be buyer or seller
        // Also used to check review eligibility before showing the review form
        // ─────────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransactionById(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized(new { message = "User identity not found in token." });

            var transaction = await _transactionService.GetTransactionByIdAsync(id, userId.Value);

            if (transaction == null)
                return NotFound(new { message = "Transaction not found." });

            return Ok(transaction);
        }
    }
}