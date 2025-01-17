using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ColumnsUpdated : Migration
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

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_UserId",
                schema: "dbo",
                table: "Subscriptions",
                newName: "IX_Subscriptions_SubscriptionActivatorId");

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

            migrationBuilder.AddColumn<int>(
                name: "SubscriptionType",
                schema: "dbo",
                table: "Subscriptions",
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
                name: "FK_Subscriptions_Users_SubscriberId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_SubscriptionActivatorId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_SubscriberId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_LevyPayments_ChairmanId",
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
                name: "Gender",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBlocked",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ProofOfPayment",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SubscriberId",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "SubscriptionType",
                schema: "dbo",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "LGA",
                schema: "dbo",
                table: "LocalGovernments");

            migrationBuilder.DropColumn(
                name: "ChairmanId",
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
