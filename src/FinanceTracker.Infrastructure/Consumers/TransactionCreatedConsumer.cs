using System.Threading.Tasks;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.Events;
using MassTransit;

namespace FinanceTracker.Infrastructure.Consumers;

public sealed class TransactionCreatedConsumer : IConsumer<TransactionCreatedIntegrationEvent>
{
    private readonly ICacheService _cacheService;

    public TransactionCreatedConsumer(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public async Task Consume(ConsumeContext<TransactionCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        // Invalidate cached summaries and transactions for this user
        await _cacheService.RemoveByPatternAsync($"summary:{message.UserId}:*", context.CancellationToken);
        await _cacheService.RemoveByPatternAsync($"transactions:{message.UserId}:*", context.CancellationToken);
    }
}

