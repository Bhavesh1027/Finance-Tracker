using FinanceTracker.API.Hubs;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.DTOs;
using Microsoft.AspNetCore.SignalR;

namespace FinanceTracker.API.Services;

public sealed class SignalRNotificationService : IBudgetNotificationService
{
    private readonly IHubContext<BudgetHub> _hubContext;

    public SignalRNotificationService(IHubContext<BudgetHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task SendBudgetAlertAsync(Guid userId, BudgetAlertDto alert, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("BudgetExceeded", alert, cancellationToken);
    }

    public Task SendTransactionConfirmedAsync(Guid userId, TransactionDto transaction, CancellationToken cancellationToken = default)
    {
        return _hubContext.Clients
            .Group(userId.ToString())
            .SendAsync("TransactionAdded", transaction, cancellationToken);
    }
}

