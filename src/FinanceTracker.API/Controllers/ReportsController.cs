using System.Net.Mime;
using FinanceTracker.Application.Abstractions;
using FinanceTracker.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>
/// Provides analytical finance reports.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ISender _sender;

    public ReportsController(IReportService reportService, ISender sender)
    {
        _reportService = reportService;
        _sender = sender;
    }

    /// <summary>
    /// Gets monthly trend data for the specified number of past months.
    /// </summary>
    /// <param name="months">Number of months to include in the trend.</param>
    /// <returns>List of monthly trend records.</returns>
    /// <response code="200">Returns the monthly trend.</response>
    /// <response code="400">If parameters are invalid.</response>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(List<MonthlyTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTrendsAsync([FromQuery] int months = 6)
    {
        if (months <= 0)
        {
            return BadRequest("Months must be greater than zero.");
        }

        if (!Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value, out var userId))
        {
            return BadRequest("User identifier not found in token.");
        }

        var result = await _reportService.GetMonthlyTrendAsync(userId, months);

        return Ok(result);
    }

    /// <summary>
    /// Gets spending insight analytics for the current user.
    /// </summary>
    /// <returns>Spending insights.</returns>
    /// <response code="200">Returns the spending insights.</response>
    /// <response code="400">If user context is invalid.</response>
    [HttpGet("insights")]
    [ProducesResponseType(typeof(SpendingInsightDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetInsightsAsync()
    {
        if (!Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value, out var userId))
        {
            return BadRequest("User identifier not found in token.");
        }

        var result = await _reportService.GetSpendingInsightAsync(userId);

        return Ok(result);
    }
}

