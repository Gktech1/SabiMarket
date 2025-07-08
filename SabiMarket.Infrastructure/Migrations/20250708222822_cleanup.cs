using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class cleanup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SowFoodCompanySalesRecords");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyStaffAppraisers");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyStaffAttendances");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyCustomers");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyProductionItems");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyShelfItems");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyStaff");

            migrationBuilder.DropTable(
                name: "SowFoodCompanies");

            migrationBuilder.AddColumn<string>(
                name: "UserType",
                table: "SubscriptionPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserType",
                table: "SubscriptionPlans");

            migrationBuilder.CreateTable(
                name: "SowFoodCompanies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanies_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyCustomers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RegisteredBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyCustomers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyCustomers_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyProductionItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyProductionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyProductionItems_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyShelfItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyShelfItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyShelfItems_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyStaff",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    StaffId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaff_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaff_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanySalesRecords",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyCustomerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SowFoodCompanyProductItemId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SowFoodCompanyShelfItemId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SowFoodCompanyStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanySalesRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyCustomers_SowFoodCompanyCustomerId",
                        column: x => x.SowFoodCompanyCustomerId,
                        principalTable: "SowFoodCompanyCustomers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyProductionItems_SowFoodCompanyProductItemId",
                        column: x => x.SowFoodCompanyProductItemId,
                        principalTable: "SowFoodCompanyProductionItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyShelfItems_SowFoodCompanyShelfItemId",
                        column: x => x.SowFoodCompanyShelfItemId,
                        principalTable: "SowFoodCompanyShelfItems",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyStaff_SowFoodCompanyStaffId",
                        column: x => x.SowFoodCompanyStaffId,
                        principalTable: "SowFoodCompanyStaff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyStaffAppraisers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyStaffAppraisers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAppraisers_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAppraisers_SowFoodCompanyStaff_SowFoodCompanyStaffId",
                        column: x => x.SowFoodCompanyStaffId,
                        principalTable: "SowFoodCompanyStaff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyStaffAttendances",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConfirmedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SowFoodCompanyStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConfirmedTimeIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    LogonTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyStaffAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAttendances_AspNetUsers_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAttendances_SowFoodCompanyStaff_SowFoodCompanyStaffId",
                        column: x => x.SowFoodCompanyStaffId,
                        principalTable: "SowFoodCompanyStaff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanies_UserId",
                table: "SowFoodCompanies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyCustomers_SowFoodCompanyId",
                table: "SowFoodCompanyCustomers",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyCustomers_UserId",
                table: "SowFoodCompanyCustomers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyProductionItems_SowFoodCompanyId",
                table: "SowFoodCompanyProductionItems",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyCustomerId",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyId",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyProductItemId",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyShelfItemId",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyShelfItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyStaffId",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyShelfItems_SowFoodCompanyId",
                table: "SowFoodCompanyShelfItems",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaff_SowFoodCompanyId",
                table: "SowFoodCompanyStaff",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaff_UserId",
                table: "SowFoodCompanyStaff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAppraisers_SowFoodCompanyStaffId",
                table: "SowFoodCompanyStaffAppraisers",
                column: "SowFoodCompanyStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAppraisers_UserId",
                table: "SowFoodCompanyStaffAppraisers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAttendances_ConfirmedByUserId",
                table: "SowFoodCompanyStaffAttendances",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAttendances_SowFoodCompanyStaffId",
                table: "SowFoodCompanyStaffAttendances",
                column: "SowFoodCompanyStaffId");
        }
    }
}
