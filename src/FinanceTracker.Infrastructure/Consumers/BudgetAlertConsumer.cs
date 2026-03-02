using System.Threading.Tasks;
using FinanceTracker.Application.Events;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace FinanceTracker.Infrastructure.Consumers;

public sealed class BudgetAlertConsumer : IConsumer<BudgetExceededIntegrationEvent>
{
    private readonly ILogger<BudgetAlertConsumer> _logger;

    public BudgetAlertConsumer(ILogger<BudgetAlertConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<BudgetExceededIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Budget exceeded alert received: UserId={UserId}, Category={Category}, Spent={Spent}, Limit={Limit}",
            message.UserId,
            message.Category,
            message.Spent,
            message.Limit);

        // Placeholder for sending email, push notifications, etc.

        return Task.CompletedTask;
    }
}

