using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Repositories;

public interface ITransactionRepository : IRepository<Transaction, Guid>
{
    Task<IReadOnlyList<Transaction>> GetByUserAndMonthAsync(Guid userId, int month, int year, CancellationToken ct = default);
    Task<decimal> GetTotalByUserAndCategoryAsync(Guid userId, Category category, int month, int year, CancellationToken ct = default);
}
