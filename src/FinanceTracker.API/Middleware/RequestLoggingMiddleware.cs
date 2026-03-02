using System.Diagnostics;
using Serilog;

namespace FinanceTracker.API.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = context.Items["X-Correlation-Id"] as string ?? string.Empty;

        try
        {
            await _next(context);

            stopwatch.Stop();

            Log.Information(
                "HTTP {Method} {Path} responded {StatusCode} in {Elapsed:0.0000} ms (CorrelationId={CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.Elapsed.TotalMilliseconds,
                correlationId);
        }
        catch
        {
            stopwatch.Stop();

            Log.Error(
                "HTTP {Method} {Path} failed in {Elapsed:0.0000} ms (CorrelationId={CorrelationId})",
                context.Request.Method,
                context.Request.Path,
                stopwatch.Elapsed.TotalMilliseconds,
                correlationId);

            throw;
        }
    }
}

