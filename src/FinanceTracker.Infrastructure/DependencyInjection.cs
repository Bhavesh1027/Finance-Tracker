using System.Data;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Domain.Repositories;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Infrastructure.Reporting;
using Microsoft.Data.SqlClient;
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

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddScoped<IDbConnection>(_ => new SqlConnection(connectionString));
        services.AddScoped<IReportService, DapperReportService>();

        return services;
    }
}
