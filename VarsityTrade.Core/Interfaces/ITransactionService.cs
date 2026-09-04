using VarsityTrade.Core.DTOs.Transactions; // Provides Transaction DTOs

namespace VarsityTrade.Core.Interfaces
{
    // This interface defines the contract for all transaction operations
    public interface ITransactionService
    {
        // Get all transactions for a buyer — their purchase history
        Task<IEnumerable<TransactionResponseDto>> GetTransactionsByBuyerAsync(int buyerId);

        // Get all transactions for a seller — their sales history
        Task<IEnumerable<TransactionResponseDto>> GetTransactionsBySellerAsync(int userId);

        // Get a single transaction by ID — used to check review eligibility
        Task<TransactionResponseDto?> GetTransactionByIdAsync(int transactionId, int userId);
    }
}