using Blazored.LocalStorage;

namespace FinanceTracker.BlazorUI.Services;

public interface IThemeService
{
    Task<bool> IsDarkModeAsync();
    Task ToggleThemeAsync();
    event Action<bool>? OnThemeChanged;
}

public sealed class ThemeService : IThemeService
{
    private const string ThemePreferenceKey = "theme_preference";

    private readonly ILocalStorageService _localStorage;

    private bool _isDarkMode = true;

    public ThemeService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public event Action<bool>? OnThemeChanged;

    public async Task<bool> IsDarkModeAsync()
    {
        var stored = await _localStorage.GetItemAsync<string?>(ThemePreferenceKey);

        if (string.IsNullOrWhiteSpace(stored))
        {
            _isDarkMode = true;
            return _isDarkMode;
        }

        if (bool.TryParse(stored, out var result))
        {
            _isDarkMode = result;
            return _isDarkMode;
        }

        _isDarkMode = string.Equals(stored, "dark", StringComparison.OrdinalIgnoreCase);
        return _isDarkMode;
    }

    public async Task ToggleThemeAsync()
    {
        _isDarkMode = !_isDarkMode;
        await _localStorage.SetItemAsync(ThemePreferenceKey, _isDarkMode);
        OnThemeChanged?.Invoke(_isDarkMode);
    }
}

