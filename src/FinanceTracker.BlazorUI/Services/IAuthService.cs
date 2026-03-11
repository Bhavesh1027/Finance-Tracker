using FinanceTracker.BlazorUI.Models;

namespace FinanceTracker.BlazorUI.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetUserNameAsync();
    Task<Guid?> GetUserIdAsync();
}

