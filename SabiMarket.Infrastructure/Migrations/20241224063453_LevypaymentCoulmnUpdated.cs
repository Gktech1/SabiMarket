using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LevypaymentCoulmnUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LevyCollections",
                schema: "dbo");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "GoodBoyId",
                schema: "dbo",
                table: "LevyPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "QRCodeScanned",
                schema: "dbo",
                table: "LevyPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_LevyPayments_GoodBoyId",
                schema: "dbo",
                table: "LevyPayments",
                column: "GoodBoyId");

            migrationBuilder.AddForeignKey(
                name: "FK_LevyPayments_GoodBoys_GoodBoyId",
                schema: "dbo",
                table: "LevyPayments",
                column: "GoodBoyId",
                principalSchema: "dbo",
                principalTable: "GoodBoys",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LevyPayments_GoodBoys_GoodBoyId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropIndex(
                name: "IX_LevyPayments_GoodBoyId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "Address",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GoodBoyId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "QRCodeScanned",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.CreateTable(
                name: "LevyCollections",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodBoyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TraderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QRCodeScanned = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevyCollections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LevyCollections_GoodBoys_GoodBoyId",
                        column: x => x.GoodBoyId,
                        principalSchema: "dbo",
                        principalTable: "GoodBoys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LevyCollections_Traders_TraderId",
                        column: x => x.TraderId,
                        principalSchema: "dbo",
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LevyCollections_GoodBoyId",
                schema: "dbo",
                table: "LevyCollections",
                column: "GoodBoyId");

            migrationBuilder.CreateIndex(
                name: "IX_LevyCollections_TraderId",
                schema: "dbo",
                table: "LevyCollections",
                column: "TraderId");
        }
    }
}
