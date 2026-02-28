using System.Data;
using Dapper;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.DTOs;
using Microsoft.Data.SqlClient;

namespace FinanceTracker.Infrastructure.Reporting;

public sealed class DapperReportService : IReportService
{
    private readonly IDbConnection _connection;

    public DapperReportService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<MonthlyTrendDto>> GetMonthlyTrendAsync(Guid userId, int months, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                MONTH(Date) AS Month,
                YEAR(Date) AS Year,
                ISNULL(SUM(CASE WHEN Type = 'Income' THEN amount_value ELSE 0 END), 0) AS TotalIncome,
                ISNULL(SUM(CASE WHEN Type = 'Expense' THEN amount_value ELSE 0 END), 0) AS TotalExpenses,
                ISNULL(SUM(CASE WHEN Type = 'Income' THEN amount_value ELSE -amount_value END), 0) AS NetBalance
            FROM transactions
            WHERE UserId = @UserId
                AND Date >= DATEADD(MONTH, -@Months, GETUTCDATE())
            GROUP BY MONTH(Date), YEAR(Date)
            ORDER BY Year, Month
            """;

        var result = await ((SqlConnection)_connection).QueryAsync<MonthlyTrendDto>(
            new CommandDefinition(sql, new { userId, months }, cancellationToken: cancellationToken));

        return result.ToList();
    }

    public async Task<SpendingInsightDto> GetSpendingInsightAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        const string topCategorySql = """
            SELECT TOP 1 Category
            FROM transactions
            WHERE UserId = @UserId AND Type = 'Expense'
            GROUP BY Category
            ORDER BY SUM(amount_value) DESC
            """;

        const string averageDailySpendSql = """
            SELECT ISNULL(
                SUM(amount_value) * 1.0 / NULLIF(DAY(EOMONTH(GETUTCDATE())), 0),
                0
            )
            FROM transactions
            WHERE UserId = @UserId AND Type = 'Expense'
                AND MONTH(Date) = MONTH(GETUTCDATE())
                AND YEAR(Date) = YEAR(GETUTCDATE())
            """;

        const string biggestTransactionSql = """
            SELECT ISNULL(MAX(amount_value), 0)
            FROM transactions
            WHERE UserId = @UserId AND Type = 'Expense'
            """;

        const string busiestDaySql = """
            SELECT TOP 1
                CASE DATEPART(weekday, Date)
                    WHEN 1 THEN 'Sunday'
                    WHEN 2 THEN 'Monday'
                    WHEN 3 THEN 'Tuesday'
                    WHEN 4 THEN 'Wednesday'
                    WHEN 5 THEN 'Thursday'
                    WHEN 6 THEN 'Friday'
                    WHEN 7 THEN 'Saturday'
                END AS DayOfWeek
            FROM transactions
            WHERE UserId = @UserId AND Type = 'Expense'
            GROUP BY DATEPART(weekday, Date)
            ORDER BY SUM(amount_value) DESC
            """;

        var sqlConnection = (SqlConnection)_connection;
        var parameters = new { userId };
        var topCategoryTask = sqlConnection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(topCategorySql, parameters, cancellationToken: cancellationToken));

        var averageDailyTask = sqlConnection.QuerySingleAsync<decimal>(
            new CommandDefinition(averageDailySpendSql, parameters, cancellationToken: cancellationToken));

        var biggestTransactionTask = sqlConnection.QuerySingleAsync<decimal>(
            new CommandDefinition(biggestTransactionSql, parameters, cancellationToken: cancellationToken));

        var busiestDayTask = sqlConnection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(busiestDaySql, parameters, cancellationToken: cancellationToken));

        await Task.WhenAll(topCategoryTask, averageDailyTask, biggestTransactionTask, busiestDayTask);

        return new SpendingInsightDto(
            TopCategory: await topCategoryTask ?? "None",
            AverageDailySpend: await averageDailyTask,
            BiggestTransaction: await biggestTransactionTask,
            BusiestDayOfWeek: await busiestDayTask ?? "None");
    }
}
