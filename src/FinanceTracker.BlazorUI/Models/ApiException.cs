using System.Net;

namespace FinanceTracker.BlazorUI.Models;

public sealed class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? ResponseContent { get; }
    public IReadOnlyList<string>? Errors { get; }

    public ApiException(HttpStatusCode statusCode, string message, string? responseContent = null, IReadOnlyList<string>? errors = null)
        : base(message)
    {
        StatusCode = statusCode;
        ResponseContent = responseContent;
        Errors = errors;
    }
}

