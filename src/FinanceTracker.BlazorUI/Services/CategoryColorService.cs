using FinanceTracker.Domain.Enums;
using MudBlazor;

namespace FinanceTracker.BlazorUI.Services;

public interface ICategoryColorService
{
    Color GetColor(Category category);
    string GetEmoji(Category category);
}

public sealed class CategoryColorService : ICategoryColorService
{
    public Color GetColor(Category category) =>
        category switch
        {
            Category.Food => Color.Primary,
            Category.Transportation => Color.Warning,
            Category.Shopping => Color.Secondary,
            Category.Housing => Color.Info,
            Category.Utilities => Color.Tertiary,
            Category.Healthcare => Color.Success,
            Category.Education => Color.Info,
            Category.Savings => Color.Success,
            Category.Investment => Color.Primary,
            Category.Entertainment => Color.Secondary,
            _ => Color.Default
        };

    public string GetEmoji(Category category) =>
        category switch
        {
            Category.Food => "🍔",
            Category.Transportation => "🚗",
            Category.Shopping => "🛍️",
            Category.Housing => "🏠",
            Category.Utilities => "💡",
            Category.Healthcare => "💊",
            Category.Education => "📚",
            Category.Savings => "💰",
            Category.Investment => "📈",
            Category.Entertainment => "🎮",
            _ => "💡"
        };
}

