using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class currencytype : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Currency",
                table: "SubscriptionPlans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "SubscriptionPlans");
        }
    }
}
