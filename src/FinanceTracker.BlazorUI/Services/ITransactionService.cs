using FinanceTracker.BlazorUI.Models;

namespace FinanceTracker.BlazorUI.Services;

public interface ITransactionService
{
    Task<List<TransactionDto>> GetTransactionsAsync(int month, int year);
    Task<MonthlySummaryDto> GetSummaryAsync(int month, int year);
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionRequest request);
}

