using FinanceTracker.Domain.Enums;

namespace FinanceTracker.BlazorUI.Models;

public sealed class TransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Category Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TransactionType TransactionType { get; set; }
}

