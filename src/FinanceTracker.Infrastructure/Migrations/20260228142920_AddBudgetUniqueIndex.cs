using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBudgetUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_budgets_userid_category_month_year",
                table: "budgets",
                columns: new[] { "UserId", "Category", "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_budgets_userid_category_month_year",
                table: "budgets");
        }
    }
}
