using FinanceTracker.Application.DTOs;

namespace FinanceTracker.Application.Abstractions;

public interface IBudgetNotificationService
{
    Task SendBudgetAlertAsync(Guid userId, BudgetAlertDto alert, CancellationToken cancellationToken = default);

    Task SendTransactionConfirmedAsync(Guid userId, TransactionDto transaction, CancellationToken cancellationToken = default);
}

