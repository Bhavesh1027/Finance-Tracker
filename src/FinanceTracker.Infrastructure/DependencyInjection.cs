using System.Data;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Domain.Repositories;
using FinanceTracker.Infrastructure.Data;
using FinanceTracker.Infrastructure.Repositories;
using FinanceTracker.Infrastructure.Reporting;
using FinanceTracker.Infrastructure.Caching;
using FinanceTracker.Infrastructure.Consumers;
using FinanceTracker.Infrastructure.Messaging;
using MassTransit;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

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

        var redisConfiguration = configuration.GetConnectionString("Redis") 
                                ?? configuration["Redis:Configuration"];

        if (!string.IsNullOrWhiteSpace(redisConfiguration))
        {
            services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(redisConfiguration));

            services.AddScoped<ICacheService, RedisCacheService>();
        }

        services.AddMassTransit(x =>
        {
            x.AddConsumer<BudgetAlertConsumer>();
            x.AddConsumer<TransactionCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var rabbitMqHost = configuration["RabbitMQ:Host"] ?? "rabbitmq://localhost";
                var rabbitMqUser = configuration["RabbitMQ:Username"] ?? "guest";
                var rabbitMqPass = configuration["RabbitMQ:Password"] ?? "guest";

                cfg.Host(new Uri(rabbitMqHost), h =>
                {
                    h.Username(rabbitMqUser);
                    h.Password(rabbitMqPass);
                });

                cfg.ReceiveEndpoint("budget-alerts", e =>
                {
                    e.ConfigureConsumer<BudgetAlertConsumer>(context);
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });

                cfg.ReceiveEndpoint("transaction-created", e =>
                {
                    e.ConfigureConsumer<TransactionCreatedConsumer>(context);
                    e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
                });
            });
        });

        services.AddScoped<IEventBus, MassTransitEventBus>();

        return services;
    }
}
