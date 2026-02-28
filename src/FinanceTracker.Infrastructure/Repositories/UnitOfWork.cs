using FinanceTracker.Domain.Repositories;
using FinanceTracker.Infrastructure.Data;

namespace FinanceTracker.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);

    public void Dispose()
    {
        _context.Dispose();
    }
}
