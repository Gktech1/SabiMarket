using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class columnModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                schema: "dbo",
                table: "LevyPayments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                schema: "dbo",
                table: "GoodBoys",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "dbo",
                table: "GoodBoys");
        }
    }
}
