using FinanceTracker.Domain.Common;
using FinanceTracker.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Infrastructure.Data;

public sealed class AppDbContext : DbContext
{
    private readonly IMediator? _mediator;

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator? mediator = null)
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Budget> Budgets => Set<Budget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEvents = new List<IDomainEvent>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Entity<Guid> entityWithEvents)
            {
                domainEvents.AddRange(entityWithEvents.DomainEvents);
                entityWithEvents.ClearDomainEvents();
            }
        }

        foreach (var domainEvent in domainEvents)
        {
            if (_mediator is not null && domainEvent is INotification notification)
            {
                await _mediator.Publish(notification, cancellationToken);
            }
        }
    }
}
