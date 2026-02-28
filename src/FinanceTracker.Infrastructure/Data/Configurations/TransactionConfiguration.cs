using FinanceTracker.Domain.Entities;
using FinanceTracker.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Data.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .ValueGeneratedNever();

        builder.OwnsOne(t => t.Amount, amount =>
        {
            amount.Property(a => a.Amount)
                .HasColumnName("amount_value")
                .HasPrecision(18, 2);

            amount.Property(a => a.Currency)
                .HasColumnName("amount_currency")
                .HasMaxLength(3);
        });

        builder.Property(t => t.Category)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Date);

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(t => t.UserId);

        builder.HasIndex(t => new { t.UserId, t.Date })
            .HasDatabaseName("IX_transactions_userid_date");
    }
}
