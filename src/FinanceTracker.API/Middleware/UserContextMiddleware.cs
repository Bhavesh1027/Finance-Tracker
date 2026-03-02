using Serilog.Context;

namespace FinanceTracker.API.Middleware;

public sealed class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = context.User?.FindFirst("sub")?.Value ?? context.User?.FindFirst("userId")?.Value;

        if (!string.IsNullOrWhiteSpace(userId))
        {
            using (LogContext.PushProperty("UserId", userId))
            {
                await _next(context);
                return;
            }
        }

        await _next(context);
    }
}

