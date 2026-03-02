using System.Net.Mime;
using FinanceTracker.Application.Commands.Transactions;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Queries.Transactions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>
/// Manages user transactions.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public sealed class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISender _sender;

    public TransactionsController(IMediator mediator, ISender sender)
    {
        _mediator = mediator;
        _sender = sender;
    }

    /// <summary>
    /// Creates a new transaction.
    /// </summary>
    /// <param name="command">Transaction details.</param>
    /// <returns>The created transaction.</returns>
    /// <response code="201">Returns the created transaction.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateTransactionCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(command);

        return CreatedAtAction(
            nameof(GetByMonthAsync),
            new { version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0", month = result.Date.Month, year = result.Date.Year },
            result);
    }

    /// <summary>
    /// Gets transactions for a given month and year.
    /// </summary>
    /// <param name="month">Month number (1-12).</param>
    /// <param name="year">Year.</param>
    /// <returns>List of transactions.</returns>
    /// <response code="200">Returns the list of transactions.</response>
    /// <response code="400">If parameters are invalid.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByMonthAsync([FromQuery] int month, [FromQuery] int year)
    {
        if (month is < 1 or > 12 || year <= 0)
        {
            return BadRequest("Invalid month or year.");
        }

        if (!Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value, out var userId))
        {
            return BadRequest("User identifier not found in token.");
        }

        var query = new GetTransactionsByMonthQuery(userId, month, year);
        var result = await _sender.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Gets a monthly summary for a given month and year.
    /// </summary>
    /// <param name="month">Month number (1-12).</param>
    /// <param name="year">Year.</param>
    /// <returns>Monthly summary.</returns>
    /// <response code="200">Returns the monthly summary.</response>
    /// <response code="400">If parameters are invalid.</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(MonthlySummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSummaryAsync([FromQuery] int month, [FromQuery] int year)
    {
        if (month is < 1 or > 12 || year <= 0)
        {
            return BadRequest("Invalid month or year.");
        }

        if (!Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value, out var userId))
        {
            return BadRequest("User identifier not found in token.");
        }

        var query = new GetMonthlySummaryQuery(userId, month, year);
        var result = await _sender.Send(query);

        return Ok(result);
    }
}

