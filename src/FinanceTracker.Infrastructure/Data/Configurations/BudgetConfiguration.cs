using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(b => b.LimitAmount, limit =>
        {
            limit.Property(l => l.Amount)
                .HasColumnName("limit_amount_value")
                .HasPrecision(18, 2);

            limit.Property(l => l.Currency)
                .HasColumnName("limit_amount_currency")
                .HasMaxLength(3);
        });

        builder.Property(b => b.Category)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(b => b.UserId);
        builder.Property(b => b.Month);
        builder.Property(b => b.Year);

        builder.HasIndex(b => new { b.UserId, b.Category, b.Month, b.Year })
            .IsUnique()
            .HasDatabaseName("IX_budgets_userid_category_month_year");
    }
}
