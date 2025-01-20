using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewTablesandColumnsAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "AccountNumber",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "UserId",
                schema: "dbo",
                table: "Subscriptions",
                newName: "SubscriptionActivatorId");

            migrationBuilder.RenameColumn(
                name: "PaymentMethod",
                schema: "dbo",
                table: "Subscriptions",
                newName: "SGId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_UserId",
                schema: "dbo",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscriptionActivatorId");

            migrationBuilder.AddColumn<string>(
                name: "AdminId",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                schema: "dbo",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                schema: "dbo",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAdminConfirmPayment",
                schema: "dbo",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscriberConfirmPayment",
                schema: "dbo",
                table: "Subscriptions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProofOfPayment",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SubscriberId",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Roles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "dbo",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                schema: "dbo",
                table: "Roles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "Roles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ComplianceRate",
                schema: "dbo",
                table: "Markets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CompliantTraders",
                schema: "dbo",
                table: "Markets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                schema: "dbo",
                table: "Markets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "LocalGovernmentName",
                schema: "dbo",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MarketCapacity",
                schema: "dbo",
                table: "Markets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "MarketName",
                schema: "dbo",
                table: "Markets",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "NonCompliantTraders",
                schema: "dbo",
                table: "Markets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OccupancyRate",
                schema: "dbo",
                table: "Markets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentTransactions",
                schema: "dbo",
                table: "Markets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                schema: "dbo",
                table: "Markets",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRevenue",
                schema: "dbo",
                table: "Markets",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TotalTraders",
                schema: "dbo",
                table: "Markets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LGA",
                schema: "dbo",
                table: "LocalGovernments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChairmanId",
                schema: "dbo",
                table: "LevyPayments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "MarketId",
                schema: "dbo",
                table: "LevyPayments",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "dbo",
                table: "Chairmen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "dbo",
                table: "Chairmen",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketId",
                schema: "dbo",
                table: "Chairmen",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalRecords",
                schema: "dbo",
                table: "Chairmen",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ChairmanId",
                schema: "dbo",
                table: "Caretakers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                schema: "dbo",
                table: "Caretakers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ChairmanId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsBlocked",
                schema: "dbo",
                table: "AssistCenterOfficers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Admins",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegisteredLGAs = table.Column<int>(type: "int", nullable: false),
                    ActiveChairmen = table.Column<int>(type: "int", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdminLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HasDashboardAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasRoleManagementAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasTeamManagementAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasAuditLogAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastDashboardAccess = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatsLastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Admins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Activity = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Module = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Admins_AdminId",
                        column: x => x.AdminId,
                        principalSchema: "dbo",
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_AdminId",
                schema: "dbo",
                table: "Users",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriberId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_LevyPayments_ChairmanId",
                schema: "dbo",
                table: "LevyPayments",
                column: "ChairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_LevyPayments_MarketId",
                schema: "dbo",
                table: "LevyPayments",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Chairmen_MarketId",
                schema: "dbo",
                table: "Chairmen",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Caretakers_ChairmanId",
                schema: "dbo",
                table: "Caretakers",
                column: "ChairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistCenterOfficers_ChairmanId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "ChairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserId",
                schema: "dbo",
                table: "Admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AdminId",
                schema: "dbo",
                table: "AuditLogs",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                schema: "dbo",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_Name",
                schema: "dbo",
                table: "RolePermissions",
                columns: new[] { "RoleId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AssistCenterOfficers_Chairmen_ChairmanId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "ChairmanId",
                principalSchema: "dbo",
                principalTable: "Chairmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Caretakers_Chairmen_ChairmanId",
                schema: "dbo",
                table: "Caretakers",
                column: "ChairmanId",
                principalSchema: "dbo",
                principalTable: "Chairmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Chairmen_Markets_MarketId",
                schema: "dbo",
                table: "Chairmen",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LevyPayments_Chairmen_ChairmanId",
                schema: "dbo",
                table: "LevyPayments",
                column: "ChairmanId",
                principalSchema: "dbo",
                principalTable: "Chairmen",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LevyPayments_Markets_MarketId",
                schema: "dbo",
                table: "LevyPayments",
                column: "MarketId",
                principalSchema: "dbo",
                principalTable: "Markets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_SubscriberId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriberId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);  // Changed from Cascade to Restrict

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_SubscriptionActivatorId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriptionActivatorId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);  // Changed from Cascade to Restrict


            /* migrationBuilder.AddForeignKey(
                 name: "FK_Subscriptions_Users_SubscriberId",
                 schema: "dbo",
                 table: "Subscriptions",
                 column: "SubscriberId",
                 principalSchema: "dbo",
                 principalTable: "Users",
                 principalColumn: "Id",
                 onDelete: ReferentialAction.Cascade);

             migrationBuilder.AddForeignKey(
                 name: "FK_Subscriptions_Users_SubscriptionActivatorId",
                 schema: "dbo",
                 table: "Subscriptions",
                 column: "SubscriptionActivatorId",
                 principalSchema: "dbo",
                 principalTable: "Users",
                 principalColumn: "Id",
                 onDelete: ReferentialAction.Cascade);*/

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Admins_AdminId",
                schema: "dbo",
                table: "Users",
                column: "AdminId",
                principalSchema: "dbo",
                principalTable: "Admins",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssistCenterOfficers_Chairmen_ChairmanId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropForeignKey(
                name: "FK_Caretakers_Chairmen_ChairmanId",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropForeignKey(
                name: "FK_Chairmen_Markets_MarketId",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropForeignKey(
                name: "FK_LevyPayments_Chairmen_ChairmanId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_LevyPayments_Markets_MarketId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_SubscriberId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_SubscriptionActivatorId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Admins_AdminId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RolePermissions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Admins",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_Users_AdminId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_SubscriberId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_LevyPayments_ChairmanId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropIndex(
                name: "IX_LevyPayments_MarketId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropIndex(
                name: "IX_Chairmen_MarketId",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropIndex(
                name: "IX_Caretakers_ChairmanId",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropIndex(
                name: "IX_AssistCenterOfficers_ChairmanId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropColumn(
                name: "AdminId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Gender",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsAdminConfirmPayment",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "IsSubscriberConfirmPayment",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "ProofOfPayment",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SubscriberId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "dbo",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                schema: "dbo",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                schema: "dbo",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ComplianceRate",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "CompliantTraders",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "EndDate",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "LocalGovernmentName",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "MarketCapacity",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "MarketName",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "NonCompliantTraders",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "OccupancyRate",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "PaymentTransactions",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "StartDate",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "TotalRevenue",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "TotalTraders",
                schema: "dbo",
                table: "Markets");

            migrationBuilder.DropColumn(
                name: "LGA",
                schema: "dbo",
                table: "LocalGovernments");

            migrationBuilder.DropColumn(
                name: "ChairmanId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "MarketId",
                schema: "dbo",
                table: "LevyPayments");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropColumn(
                name: "MarketId",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropColumn(
                name: "TotalRecords",
                schema: "dbo",
                table: "Chairmen");

            migrationBuilder.DropColumn(
                name: "ChairmanId",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                schema: "dbo",
                table: "Caretakers");

            migrationBuilder.DropColumn(
                name: "ChairmanId",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                schema: "dbo",
                table: "AssistCenterOfficers");

            migrationBuilder.RenameColumn(
                name: "SubscriptionActivatorId",
                schema: "dbo",
                table: "Subscriptions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "SGId",
                schema: "dbo",
                table: "Subscriptions",
                newName: "PaymentMethod");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_SubscriptionActivatorId",
                schema: "dbo",
                table: "Subscriptions",
                newName: "IX_Subscriptions_UserId");

            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                schema: "dbo",
                table: "Subscriptions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "dbo",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                schema: "dbo",
                table: "Subscriptions",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
