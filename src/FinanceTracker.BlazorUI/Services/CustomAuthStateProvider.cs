using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace FinanceTracker.BlazorUI.Services;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    internal const string AuthTokenKey = "auth_token";
    internal const string RefreshTokenKey = "refresh_token";

    private readonly ILocalStorageService _localStorage;

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsync<string?>(AuthTokenKey);

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        var principal = CreatePrincipalFromToken(token);

        if (!IsTokenValid(principal))
        {
            await _localStorage.RemoveItemAsync(AuthTokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return new AuthenticationState(principal);
    }

    public void NotifyAuthStateChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    internal static ClaimsPrincipal CreatePrincipalFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwt;

        try
        {
            jwt = handler.ReadJwtToken(token);
        }
        catch
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        var claims = jwt.Claims.ToList();

        var userId = claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.NameIdentifier ||
            c.Type == "sub" ||
            c.Type == "userId")
            ?.Value;

        if (!string.IsNullOrWhiteSpace(userId) &&
            claims.All(c => c.Type != ClaimTypes.NameIdentifier))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        }

        var name = claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Name ||
            c.Type == "name" ||
            c.Type == "preferred_username" ||
            c.Type == "username")
            ?.Value;

        if (!string.IsNullOrWhiteSpace(name) &&
            claims.All(c => c.Type != ClaimTypes.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, name));
        }

        var identity = new ClaimsIdentity(claims, "jwt");
        return new ClaimsPrincipal(identity);
    }

    private static bool IsTokenValid(ClaimsPrincipal principal)
    {
        var expClaim = principal.Claims.FirstOrDefault(c => c.Type == "exp");
        if (expClaim is null)
        {
            return false;
        }

        if (!long.TryParse(expClaim.Value, out var expSeconds))
        {
            return false;
        }

        var expiry = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
        return expiry > DateTime.UtcNow;
    }
}

