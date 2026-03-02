using System.Net.Mime;
using FinanceTracker.Application.Commands.Budgets;
using FinanceTracker.Application.DTOs;
using FinanceTracker.Application.Queries.Budgets;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceTracker.API.Controllers;

/// <summary>
/// Manages user budgets.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
public sealed class BudgetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ISender _sender;

    public BudgetsController(IMediator mediator, ISender sender)
    {
        _mediator = mediator;
        _sender = sender;
    }

    /// <summary>
    /// Sets or updates a budget for a user.
    /// </summary>
    /// <param name="command">Budget details.</param>
    /// <returns>The created or updated budget.</returns>
    /// <response code="201">Returns the created or updated budget.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BudgetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetBudgetAsync([FromBody] SetBudgetCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _mediator.Send(command);

        return Created(string.Empty, result);
    }

    /// <summary>
    /// Gets budget status for a given month and year.
    /// </summary>
    /// <param name="month">Month number (1-12).</param>
    /// <param name="year">Year.</param>
    /// <returns>List of budget statuses.</returns>
    /// <response code="200">Returns the budget status list.</response>
    /// <response code="400">If parameters are invalid.</response>
    [HttpGet("status")]
    [ProducesResponseType(typeof(List<BudgetStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStatusAsync([FromQuery] int month, [FromQuery] int year)
    {
        if (month is < 1 or > 12 || year <= 0)
        {
            return BadRequest("Invalid month or year.");
        }

        if (!Guid.TryParse(User.FindFirst("sub")?.Value ?? User.FindFirst("userId")?.Value, out var userId))
        {
            return BadRequest("User identifier not found in token.");
        }

        var query = new GetBudgetStatusQuery(userId, month, year);
        var result = await _sender.Send(query);

        return Ok(result);
    }
}

