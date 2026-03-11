using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.BlazorUI.Pages.Auth;

public sealed class LoginModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}

