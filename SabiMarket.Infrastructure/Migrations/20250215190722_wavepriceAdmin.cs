using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class wavepriceAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementViews_Advertisements_AdvertisementId",
                schema: "dbo",
                table: "AdvertisementViews");

            migrationBuilder.DropForeignKey(
                name: "FK_AssistCenterOfficers_LocalGovernments_LocalGovernmentId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropForeignKey(
                name: "FK_AssistCenterOfficers_Users_UserId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Caretakers_Markets_MarketId",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropForeignKey(
                name: "FK_Caretakers_Users_UserId",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropForeignKey(
                name: "FK_Chairmen_Markets_MarketId",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodBoys_Users_UserId",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_Caretakers_CaretakerId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_MarketSections_SectionId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_Markets_MarketId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_Users_UserId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropIndex(
                name: "IX_Traders_QRCode",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropIndex(
                name: "IX_Traders_TIN",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropIndex(
                name: "IX_Chairmen_MarketId",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                schema: "dbo",
                table: "WaivedProducts",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                schema: "dbo",
                table: "WaivedProducts",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "WaivedProducts",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "WaivedProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Vendors",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "BusinessDescription",
                schema: "dbo",
                table: "Vendors",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Vendors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "QRCode",
                schema: "dbo",
                table: "Traders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Traders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "SGId",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ProofOfPayment",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SubscriptionPlanId",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ChairmanId",
                schema: "dbo",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Markets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "LocalGovernments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "LevyPayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "GoodBoys",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MarketId",
                schema: "dbo",
                table: "GoodBoys",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WaivedProductId",
                schema: "dbo",
                table: "Customers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomerId1",
                schema: "dbo",
                table: "CustomerOrders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "CustomerOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "CustomerOrderItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WaivedProductId",
                schema: "dbo",
                table: "CustomerOrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "CustomerFeedbacks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Chairmen",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Caretakers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "AuditLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "AssistCenterOfficers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MarketId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserLevel",
                schema: "dbo",
                table: "AssistCenterOfficers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                schema: "dbo",
                table: "AdvertisementViews",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "AdvertisementViews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "TargetUrl",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AdminId",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdvertId",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AdvertPlacement",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BankTransferReference",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Advertisements",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentProofUrl",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PaymentStatus",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VendorId1",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasAdvertManagementAccess",
                schema: "dbo",
                table: "Admins",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "dbo",
                table: "Admins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AdvertisementLanguages",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdvertisementId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertisementLanguages_Advertisements_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalSchema: "dbo",
                        principalTable: "Advertisements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AdvertPayments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdvertisementId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ProofOfPaymentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertPayments_Advertisements_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalSchema: "dbo",
                        principalTable: "Advertisements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanies",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanies_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPlans",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Frequency = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPlans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyCustomers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RegisteredBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyCustomers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyCustomers_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyCustomers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyProductionItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyProductionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyProductionItems_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyShelfItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyShelfItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyShelfItems_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyStaff",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StaffId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaff_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaff_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanySalesRecords",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SowFoodCompanyProductItemId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SowFoodCompanyShelfItemId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SowFoodCompanyCustomerId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SowFoodCompanyStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanySalesRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanies_SowFoodCompanyId",
                        column: x => x.SowFoodCompanyId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyCustomers_SowFoodCompanyCustomerId",
                        column: x => x.SowFoodCompanyCustomerId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanyCustomers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyProductionItems_SowFoodCompanyProductItemId",
                        column: x => x.SowFoodCompanyProductItemId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanyProductionItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyShelfItems_SowFoodCompanyShelfItemId",
                        column: x => x.SowFoodCompanyShelfItemId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanyShelfItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanySalesRecords_SowFoodCompanyStaff_SowFoodCompanyStaffId",
                        column: x => x.SowFoodCompanyStaffId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanyStaff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyStaffAppraisers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Remark = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyStaffAppraisers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAppraisers_SowFoodCompanyStaff_SowFoodCompanyStaffId",
                        column: x => x.SowFoodCompanyStaffId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanyStaff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAppraisers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SowFoodCompanyStaffAttendances",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SowFoodCompanyStaffId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LogonTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogoutTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedTimeIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    ConfirmedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SowFoodCompanyStaffAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAttendances_SowFoodCompanyStaff_SowFoodCompanyStaffId",
                        column: x => x.SowFoodCompanyStaffId,
                        principalSchema: "dbo",
                        principalTable: "SowFoodCompanyStaff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SowFoodCompanyStaffAttendances_Users_ConfirmedByUserId",
                        column: x => x.ConfirmedByUserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionPlanId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodBoys_MarketId",
                schema: "dbo",
                table: "GoodBoys",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_WaivedProductId",
                schema: "dbo",
                table: "Customers",
                column: "WaivedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerId1",
                schema: "dbo",
                table: "CustomerOrders",
                column: "CustomerId1",
                unique: true,
                filter: "[CustomerId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Chairmen_MarketId",
                schema: "dbo",
                table: "Chairmen",
                column: "MarketId",
                unique: true,
                filter: "[MarketId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssistCenterOfficers_MarketId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_AdminId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_VendorId1",
                schema: "dbo",
                table: "Advertisements",
                column: "VendorId1");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementLanguages_AdvertisementId",
                schema: "dbo",
                table: "AdvertisementLanguages",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertPayments_AdvertisementId",
                schema: "dbo",
                table: "AdvertPayments",
                column: "AdvertisementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanies_UserId",
                schema: "dbo",
                table: "SowFoodCompanies",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyCustomers_SowFoodCompanyId",
                schema: "dbo",
                table: "SowFoodCompanyCustomers",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyCustomers_UserId",
                schema: "dbo",
                table: "SowFoodCompanyCustomers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyProductionItems_SowFoodCompanyId",
                schema: "dbo",
                table: "SowFoodCompanyProductionItems",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyCustomerId",
                schema: "dbo",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyCustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyId",
                schema: "dbo",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyProductItemId",
                schema: "dbo",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyProductItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyShelfItemId",
                schema: "dbo",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyShelfItemId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanySalesRecords_SowFoodCompanyStaffId",
                schema: "dbo",
                table: "SowFoodCompanySalesRecords",
                column: "SowFoodCompanyStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyShelfItems_SowFoodCompanyId",
                schema: "dbo",
                table: "SowFoodCompanyShelfItems",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaff_SowFoodCompanyId",
                schema: "dbo",
                table: "SowFoodCompanyStaff",
                column: "SowFoodCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaff_UserId",
                schema: "dbo",
                table: "SowFoodCompanyStaff",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAppraisers_SowFoodCompanyStaffId",
                schema: "dbo",
                table: "SowFoodCompanyStaffAppraisers",
                column: "SowFoodCompanyStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAppraisers_UserId",
                schema: "dbo",
                table: "SowFoodCompanyStaffAppraisers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAttendances_ConfirmedByUserId",
                schema: "dbo",
                table: "SowFoodCompanyStaffAttendances",
                column: "ConfirmedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SowFoodCompanyStaffAttendances_SowFoodCompanyStaffId",
                schema: "dbo",
                table: "SowFoodCompanyStaffAttendances",
                column: "SowFoodCompanyStaffId");

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Admins_AdminId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdminId",
                principalSchema: "dbo",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Advertisements_Vendors_VendorId1",
                schema: "dbo",
                table: "Advertisements",
                column: "VendorId1",
                principalSchema: "dbo",
                principalTable: "Vendors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementViews_Advertisements_AdvertisementId",
                schema: "dbo",
                table: "AdvertisementViews",
                column: "AdvertisementId",
                principalSchema: "dbo",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssistCenterOfficers_LocalGovernments_LocalGovernmentId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "LocalGovernmentId",
                principalSchema: "dbo",
                principalTable: "LocalGovernments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssistCenterOfficers_Markets_MarketId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssistCenterOfficers_Users_UserId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                schema: "dbo",
                table: "AuditLogs",
                column: "AdminId",
                principalSchema: "dbo",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Caretakers_Markets_MarketId",
                schema: "dbo",
                table: "Caretakers",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Caretakers_Users_UserId",
                schema: "dbo",
                table: "Caretakers",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chairmen_Markets_MarketId",
                schema: "dbo",
                table: "Chairmen",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerOrders_Customers_CustomerId1",
                schema: "dbo",
                table: "CustomerOrders",
                column: "CustomerId1",
                principalSchema: "dbo",
                principalTable: "Customers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_WaivedProducts_WaivedProductId",
                schema: "dbo",
                table: "Customers",
                column: "WaivedProductId",
                principalSchema: "dbo",
                principalTable: "WaivedProducts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                schema: "dbo",
                table: "GoodBoys",
                column: "CaretakerId",
                principalSchema: "dbo",
                principalTable: "Caretakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodBoys_Markets_MarketId",
                schema: "dbo",
                table: "GoodBoys",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodBoys_Users_UserId",
                schema: "dbo",
                table: "GoodBoys",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriptionPlanId",
                principalSchema: "dbo",
                principalTable: "SubscriptionPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_Caretakers_CaretakerId",
                schema: "dbo",
                table: "Traders",
                column: "CaretakerId",
                principalSchema: "dbo",
                principalTable: "Caretakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_MarketSections_SectionId",
                schema: "dbo",
                table: "Traders",
                column: "SectionId",
                principalSchema: "dbo",
                principalTable: "MarketSections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_Markets_MarketId",
                schema: "dbo",
                table: "Traders",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_Users_UserId",
                schema: "dbo",
                table: "Traders",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Admins_AdminId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_Advertisements_Vendors_VendorId1",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertisementViews_Advertisements_AdvertisementId",
                schema: "dbo",
                table: "AdvertisementViews");

            migrationBuilder.DropForeignKey(
                name: "FK_AssistCenterOfficers_LocalGovernments_LocalGovernmentId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropForeignKey(
                name: "FK_AssistCenterOfficers_Markets_MarketId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropForeignKey(
                name: "FK_AssistCenterOfficers_Users_UserId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Caretakers_Markets_MarketId",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropForeignKey(
                name: "FK_Caretakers_Users_UserId",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropForeignKey(
                name: "FK_Chairmen_Markets_MarketId",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerOrders_Customers_CustomerId1",
                schema: "dbo",
                table: "CustomerOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_WaivedProducts_WaivedProductId",
                schema: "dbo",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodBoys_Markets_MarketId",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropForeignKey(
                name: "FK_GoodBoys_Users_UserId",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_Caretakers_CaretakerId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_MarketSections_SectionId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_Markets_MarketId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropForeignKey(
                name: "FK_Traders_Users_UserId",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropTable(
                name: "AdvertisementLanguages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AdvertPayments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanySalesRecords",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyStaffAppraisers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyStaffAttendances",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SubscriptionPlans",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyCustomers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyProductionItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyShelfItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanyStaff",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanies",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_SubscriptionPlanId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_GoodBoys_MarketId",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropIndex(
                name: "IX_Customers_WaivedProductId",
                schema: "dbo",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerOrders_CustomerId1",
                schema: "dbo",
                table: "CustomerOrders");

            migrationBuilder.DropIndex(
                name: "IX_Chairmen_MarketId",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropIndex(
                name: "IX_AssistCenterOfficers_MarketId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_AdminId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropIndex(
                name: "IX_Advertisements_VendorId1",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "WaivedProducts");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Vendors");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Traders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SubscriptionPlanId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "ChairmanId",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "LocalGovernments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropColumn(
                name: "MarketId",
                schema: "dbo",
                table: "GoodBoys");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "WaivedProductId",
                schema: "dbo",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CustomerId1",
                schema: "dbo",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "CustomerOrders");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "CustomerOrderItems");

            migrationBuilder.DropColumn(
                name: "WaivedProductId",
                schema: "dbo",
                table: "CustomerOrderItems");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "CustomerFeedbacks");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropColumn(
                name: "MarketId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropColumn(
                name: "UserLevel",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "AdvertisementViews");

            migrationBuilder.DropColumn(
                name: "AdminId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AdvertId",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "AdvertPlacement",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "BankTransferReference",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Language",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "PaymentProofUrl",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "VendorId1",
                schema: "dbo",
                table: "Advertisements");

            migrationBuilder.DropColumn(
                name: "HasAdvertManagementAccess",
                schema: "dbo",
                table: "Admins");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "dbo",
                table: "Admins");

            migrationBuilder.AlterColumn<string>(
                name: "ProductCode",
                schema: "dbo",
                table: "WaivedProducts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                schema: "dbo",
                table: "WaivedProducts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "WaivedProducts",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Vendors",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "BusinessDescription",
                schema: "dbo",
                table: "Vendors",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "QRCode",
                schema: "dbo",
                table: "Traders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SGId",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ProofOfPayment",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "IPAddress",
                schema: "dbo",
                table: "AdvertisementViews",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "TargetUrl",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Advertisements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.CreateIndex(
                name: "IX_Traders_QRCode",
                schema: "dbo",
                table: "Traders",
                column: "QRCode",
                unique: true,
                filter: "[QRCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_TIN",
                schema: "dbo",
                table: "Traders",
                column: "TIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chairmen_MarketId",
                schema: "dbo",
                table: "Chairmen",
                column: "MarketId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertisementViews_Advertisements_AdvertisementId",
                schema: "dbo",
                table: "AdvertisementViews",
                column: "AdvertisementId",
                principalSchema: "dbo",
                principalTable: "Advertisements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssistCenterOfficers_LocalGovernments_LocalGovernmentId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "LocalGovernmentId",
                principalSchema: "dbo",
                principalTable: "LocalGovernments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AssistCenterOfficers_Users_UserId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_Admins_AdminId",
                schema: "dbo",
                table: "AuditLogs",
                column: "AdminId",
                principalSchema: "dbo",
                principalTable: "Admins",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Caretakers_Markets_MarketId",
                schema: "dbo",
                table: "Caretakers",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Caretakers_Users_UserId",
                schema: "dbo",
                table: "Caretakers",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chairmen_Markets_MarketId",
                schema: "dbo",
                table: "Chairmen",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GoodBoys_Caretakers_CaretakerId",
                schema: "dbo",
                table: "GoodBoys",
                column: "CaretakerId",
                principalSchema: "dbo",
                principalTable: "Caretakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GoodBoys_Users_UserId",
                schema: "dbo",
                table: "GoodBoys",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_Caretakers_CaretakerId",
                schema: "dbo",
                table: "Traders",
                column: "CaretakerId",
                principalSchema: "dbo",
                principalTable: "Caretakers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_MarketSections_SectionId",
                schema: "dbo",
                table: "Traders",
                column: "SectionId",
                principalSchema: "dbo",
                principalTable: "MarketSections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_Markets_MarketId",
                schema: "dbo",
                table: "Traders",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Traders_Users_UserId",
                schema: "dbo",
                table: "Traders",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
