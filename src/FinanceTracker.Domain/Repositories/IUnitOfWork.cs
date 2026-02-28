namespace FinanceTracker.Domain.Repositories;

public interface IUnitOfWork : IDisposable
{
    Task SaveChangesAsync(CancellationToken ct = default);
}
