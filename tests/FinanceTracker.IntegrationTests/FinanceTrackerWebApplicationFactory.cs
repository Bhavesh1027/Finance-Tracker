using FinanceTracker.API;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.Events;
using FinanceTracker.Infrastructure.Data;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.IntegrationTests;

public sealed class FinanceTrackerWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public FinanceTrackerWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace AppDbContext with SQLite in-memory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Replace MassTransit with in-memory test harness
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<FinanceTracker.Infrastructure.Consumers.BudgetAlertConsumer>();
                cfg.AddConsumer<FinanceTracker.Infrastructure.Consumers.TransactionCreatedConsumer>();

                cfg.AddConsumerTestHarness<FinanceTracker.Infrastructure.Consumers.BudgetAlertConsumer>();
                cfg.AddConsumerTestHarness<FinanceTracker.Infrastructure.Consumers.TransactionCreatedConsumer>();
            });

            // Replace ICacheService with a no-op mock
            var cacheDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ICacheService));
            if (cacheDescriptor is not null)
            {
                services.Remove(cacheDescriptor);
            }

            services.AddSingleton<ICacheService, NoOpCacheService>();

            // Ensure database created
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}

public sealed class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) => Task.FromResult<T?>(default);

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default) => Task.CompletedTask;
}

