using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class purchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_WaivedProducts_WaivedProductId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_WaivedProductId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "WaivedProductId",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "CustomerId",
                table: "WaivedProducts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CustomerWaiveProductPurchases",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WaivedProductId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerWaiveProductPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerWaiveProductPurchases_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerWaiveProductPurchases_WaivedProducts_WaivedProductId",
                        column: x => x.WaivedProductId,
                        principalTable: "WaivedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WaivedProducts_CustomerId",
                table: "WaivedProducts",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWaiveProductPurchases_CustomerId",
                table: "CustomerWaiveProductPurchases",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerWaiveProductPurchases_WaivedProductId",
                table: "CustomerWaiveProductPurchases",
                column: "WaivedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_WaivedProducts_Customers_CustomerId",
                table: "WaivedProducts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WaivedProducts_Customers_CustomerId",
                table: "WaivedProducts");

            migrationBuilder.DropTable(
                name: "CustomerWaiveProductPurchases");

            migrationBuilder.DropIndex(
                name: "IX_WaivedProducts_CustomerId",
                table: "WaivedProducts");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "WaivedProducts");

            migrationBuilder.AddColumn<string>(
                name: "WaivedProductId",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_WaivedProductId",
                table: "Customers",
                column: "WaivedProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_WaivedProducts_WaivedProductId",
                table: "Customers",
                column: "WaivedProductId",
                principalTable: "WaivedProducts",
                principalColumn: "Id");
        }
    }
}
