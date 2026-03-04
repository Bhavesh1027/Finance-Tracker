using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog;

namespace FinanceTracker.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ProblemDetailsFactory problemDetailsFactory,
        IWebHostEnvironment environment)
    {
        _next = next;
        _problemDetailsFactory = problemDetailsFactory;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["X-Correlation-Id"] as string ?? string.Empty;

        Log.Error(exception, "Unhandled exception {@CorrelationId}", correlationId);

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException validationException => (
                (int)HttpStatusCode.BadRequest,
                "Validation failed",
                "One or more validation errors occurred.",
                validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            UnauthorizedAccessException => (
                (int)HttpStatusCode.Unauthorized,
                "Unauthorized",
                "You are not authorized to perform this action.",
                (IDictionary<string, string[]>?)null
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An error occurred",
                _environment.IsDevelopment() ? exception.ToString() : "An unexpected error occurred.",
                (IDictionary<string, string[]>?)null
            )
        };

        // Custom NotFoundException (if you add one later)
        if (exception.GetType().Name == "NotFoundException")
        {
            statusCode = (int)HttpStatusCode.NotFound;
            title = "Resource not found";
            detail = exception.Message;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        var problemDetails = errors is not null
            ? _problemDetailsFactory.CreateValidationProblemDetails(
                context,
                new ModelStateDictionary(),
                statusCode: statusCode,
                title: title,
                detail: detail,
                instance: context.Request.Path)
            : _problemDetailsFactory.CreateProblemDetails(
                context,
                statusCode: statusCode,
                title: title,
                detail: detail,
                instance: context.Request.Path);

        problemDetails.Extensions["correlationId"] = correlationId;

        if (errors is not null)
        {
            foreach (var kvp in errors)
            {
                problemDetails.Extensions[$"errors:{kvp.Key}"] = kvp.Value;
            }
        }

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }
}

