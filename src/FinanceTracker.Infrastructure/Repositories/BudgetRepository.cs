using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Repositories;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public sealed class BudgetRepository : IBudgetRepository
{
    private readonly AppDbContext _context;

    public BudgetRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Budgets.FindAsync([id], ct);

    public async Task<IReadOnlyList<Budget>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Budgets.ToListAsync(ct);

    public async Task AddAsync(Budget entity, CancellationToken ct = default)
    {
        await _context.Budgets.AddAsync(entity, ct);
    }

    public Task UpdateAsync(Budget entity, CancellationToken ct = default)
    {
        _context.Budgets.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
            _context.Budgets.Remove(entity);
    }

    public async Task<Budget?> GetByUserCategoryMonthAsync(Guid userId, Category category, int month, int year, CancellationToken ct = default) =>
        await _context.Budgets
            .FirstOrDefaultAsync(b => b.UserId == userId && b.Category == category && b.Month == month && b.Year == year, ct);
}
