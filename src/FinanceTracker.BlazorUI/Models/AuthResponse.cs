namespace FinanceTracker.BlazorUI.Models;

public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

