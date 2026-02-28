using FinanceTracker.Domain.Repositories;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceTracker.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
