using System.Linq;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.Commands.Transactions;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using FinanceTracker.Domain.Repositories;
using FinanceTracker.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace FinanceTracker.UnitTests;

public sealed class CreateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _transactionRepositoryMock = new();
    private readonly Mock<IBudgetRepository> _budgetRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICacheService> _cacheServiceMock = new();

    private CreateTransactionCommandHandler CreateHandler()
        => new(_transactionRepositoryMock.Object, _budgetRepositoryMock.Object, _unitOfWorkMock.Object, _cacheServiceMock.Object);

    [Fact]
    public async Task Should_Create_Transaction_WhenValidInput()
    {
        var handler = CreateHandler();
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            100m,
            "usd",
            Category.Food,
            "Lunch",
            DateTime.UtcNow,
            TransactionType.Expense);

        var result = await handler.Handle(command, CancellationToken.None);

        _transactionRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Transaction>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheServiceMock.Verify(c => c.RemoveByPatternAsync($"summary:{command.UserId}:*", It.IsAny<CancellationToken>()), Times.Once);

        result.Amount.Should().Be(100m);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenAmountIsZero()
    {
        var handler = CreateHandler();
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            0m,
            "USD",
            Category.Food,
            "Invalid",
            DateTime.UtcNow,
            TransactionType.Expense);

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>()
            .WithMessage("Transaction amount must be greater than zero.");
    }

    [Fact]
    public async Task Should_ThrowValidationException_WhenDateIsInFuture()
    {
        var handler = CreateHandler();
        var futureDate = DateTime.UtcNow.AddDays(1);
        var command = new CreateTransactionCommand(
            Guid.NewGuid(),
            50m,
            "USD",
            Category.Food,
            "Future",
            futureDate,
            TransactionType.Expense);

        Func<Task> act = async () =>
        {
            var amount = Money.Create(command.Amount, command.Currency);
            _ = Transaction.Create(
                command.UserId,
                amount,
                command.Category,
                command.Description,
                command.Date,
                command.TransactionType);
            await handler.Handle(command, CancellationToken.None);
        };

        await act.Should().ThrowAsync<Domain.Exceptions.DomainException>()
            .WithMessage("*future*");
    }

    [Fact]
    public async Task Should_RaiseBudgetExceededEvent_WhenBudgetLimitSurpassed()
    {
        var handler = CreateHandler();
        var userId = Guid.NewGuid();
        var command = new CreateTransactionCommand(
            userId,
            200m,
            "USD",
            Category.Food,
            "Dinner",
            DateTime.UtcNow,
            TransactionType.Expense);

        var budget = new Domain.Entities.Budget(
            Guid.NewGuid(),
            userId,
            Category.Food,
            100m,
            "USD",
            DateTime.UtcNow.Month,
            DateTime.UtcNow.Year);

        _budgetRepositoryMock
            .Setup(b => b.GetByUserCategoryMonthAsync(
                userId,
                Category.Food,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(budget);

        _transactionRepositoryMock
            .Setup(r => r.GetTotalByUserAndCategoryAsync(
                userId,
                Category.Food,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(200m);

        await handler.Handle(command, CancellationToken.None);

        budget.DomainEvents.Should().ContainSingle(e => e is Domain.Events.BudgetExceededDomainEvent);
    }

    [Fact]
    public async Task Should_NotRaiseBudgetExceededEvent_WhenNoBudgetSet()
    {
        var handler = CreateHandler();
        var userId = Guid.NewGuid();
        var command = new CreateTransactionCommand(
            userId,
            50m,
            "USD",
            Category.Food,
            "Snack",
            DateTime.UtcNow,
            TransactionType.Expense);

        _budgetRepositoryMock
            .Setup(b => b.GetByUserCategoryMonthAsync(
                userId,
                Category.Food,
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Budget?)null);

        await handler.Handle(command, CancellationToken.None);

        _budgetRepositoryMock.Verify(b => b.GetByUserCategoryMonthAsync(
            userId,
            Category.Food,
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

