using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Repositories;
using FinanceTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly AppDbContext _context;

    public TransactionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Transactions.FindAsync([id], ct);

    public async Task<IReadOnlyList<Transaction>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Transactions.ToListAsync(ct);

    public async Task AddAsync(Transaction entity, CancellationToken ct = default)
    {
        await _context.Transactions.AddAsync(entity, ct);
    }

    public Task UpdateAsync(Transaction entity, CancellationToken ct = default)
    {
        _context.Transactions.Update(entity);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        if (entity is not null)
            _context.Transactions.Remove(entity);
    }

    public async Task<IReadOnlyList<Transaction>> GetByUserAndMonthAsync(Guid userId, int month, int year, CancellationToken ct = default) =>
        await _context.Transactions
            .Where(t => t.UserId == userId && t.Date.Month == month && t.Date.Year == year)
            .OrderByDescending(t => t.Date)
            .ToListAsync(ct);

    public async Task<decimal> GetTotalByUserAndCategoryAsync(Guid userId, Category category, int month, int year, CancellationToken ct = default) =>
        await _context.Transactions
            .Where(t => t.UserId == userId && t.Category == category && t.Date.Month == month && t.Date.Year == year && t.Type == TransactionType.Expense)
            .SumAsync(t => t.Amount.Amount, ct);
}
