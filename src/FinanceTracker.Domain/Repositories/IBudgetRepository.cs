using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;

namespace FinanceTracker.Domain.Repositories;

public interface IBudgetRepository : IRepository<Budget, Guid>
{
    Task<Budget?> GetByUserCategoryMonthAsync(Guid userId, Category category, int month, int year, CancellationToken ct = default);
}
