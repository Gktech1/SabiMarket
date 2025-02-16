using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SabiMarket.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "LocalGovernments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LGA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurrentRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalGovernments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategories",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
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
                name: "Markets",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LocalGovernmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChairmanId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    MarketName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentTransactions = table.Column<int>(type: "int", nullable: false),
                    LocalGovernmentName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalTraders = table.Column<int>(type: "int", nullable: false),
                    MarketCapacity = table.Column<int>(type: "int", nullable: false),
                    OccupancyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ComplianceRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CompliantTraders = table.Column<int>(type: "int", nullable: false),
                    NonCompliantTraders = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Markets_LocalGovernments_LocalGovernmentId",
                        column: x => x.LocalGovernmentId,
                        principalSchema: "dbo",
                        principalTable: "LocalGovernments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfileImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenJwtId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshTokenExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRefreshTokenUsed = table.Column<bool>(type: "bit", nullable: true),
                    LocalGovernmentId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AdminId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_LocalGovernments_LocalGovernmentId",
                        column: x => x.LocalGovernmentId,
                        principalSchema: "dbo",
                        principalTable: "LocalGovernments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "MarketSections",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MarketSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketSections_Markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                    HasAdvertManagementAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AdminLevel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    HasDashboardAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasRoleManagementAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasTeamManagementAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasAuditLogAccess = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    LastDashboardAccess = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StatsLastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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
                name: "AspNetUserClaims",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                schema: "dbo",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalSchema: "dbo",
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                schema: "dbo",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Chairmen",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LocalGovernmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Office = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalRecords = table.Column<int>(type: "int", nullable: false),
                    TermStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TermEnd = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chairmen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Chairmen_LocalGovernments_LocalGovernmentId",
                        column: x => x.LocalGovernmentId,
                        principalSchema: "dbo",
                        principalTable: "LocalGovernments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Chairmen_Markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Chairmen_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
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
                name: "Subscriptions",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SGId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubscriberId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubscriptionActivatorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ProofOfPayment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsSubscriberConfirmPayment = table.Column<bool>(type: "bit", nullable: false),
                    IsAdminConfirmPayment = table.Column<bool>(type: "bit", nullable: false),
                    SubscriptionPlanId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_SubscriptionPlans_SubscriptionPlanId",
                        column: x => x.SubscriptionPlanId,
                        principalSchema: "dbo",
                        principalTable: "SubscriptionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_SubscriberId",
                        column: x => x.SubscriberId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Users_SubscriptionActivatorId",
                        column: x => x.SubscriptionActivatorId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vendors",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LocalGovernmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BusinessAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VendorCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    BusinessDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSubscriptionActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendors_LocalGovernments_LocalGovernmentId",
                        column: x => x.LocalGovernmentId,
                        principalSchema: "dbo",
                        principalTable: "LocalGovernments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Vendors_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "AssistCenterOfficers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChairmanId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    UserLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocalGovernmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssistCenterOfficers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssistCenterOfficers_Chairmen_ChairmanId",
                        column: x => x.ChairmanId,
                        principalSchema: "dbo",
                        principalTable: "Chairmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssistCenterOfficers_LocalGovernments_LocalGovernmentId",
                        column: x => x.LocalGovernmentId,
                        principalSchema: "dbo",
                        principalTable: "LocalGovernments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssistCenterOfficers_Markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Markets",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssistCenterOfficers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Caretakers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChairmanId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caretakers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Caretakers_Chairmen_ChairmanId",
                        column: x => x.ChairmanId,
                        principalSchema: "dbo",
                        principalTable: "Chairmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Caretakers_Markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Caretakers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Advertisements",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdminId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TargetUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AdvertId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Location = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AdvertPlacement = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PaymentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentProofUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BankTransferReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    VendorId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Advertisements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Advertisements_Admins_AdminId",
                        column: x => x.AdminId,
                        principalSchema: "dbo",
                        principalTable: "Admins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Advertisements_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "dbo",
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Advertisements_Vendors_VendorId1",
                        column: x => x.VendorId1,
                        principalSchema: "dbo",
                        principalTable: "Vendors",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "WaivedProducts",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ProductCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OriginalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WaivedPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAvailbleForUrgentPurchase = table.Column<bool>(type: "bit", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaivedProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WaivedProducts_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "dbo",
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GoodBoys",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CaretakerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoodBoys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GoodBoys_Caretakers_CaretakerId",
                        column: x => x.CaretakerId,
                        principalSchema: "dbo",
                        principalTable: "Caretakers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodBoys_Markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GoodBoys_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Traders",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SectionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CaretakerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TIN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BusinessName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BusinessType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QRCode = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Traders_Caretakers_CaretakerId",
                        column: x => x.CaretakerId,
                        principalSchema: "dbo",
                        principalTable: "Caretakers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Traders_MarketSections_SectionId",
                        column: x => x.SectionId,
                        principalSchema: "dbo",
                        principalTable: "MarketSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Traders_Markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Traders_Users_UserId",
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
                name: "AdvertisementViews",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdvertisementId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IPAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertisementViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdvertisementViews_Advertisements_AdvertisementId",
                        column: x => x.AdvertisementId,
                        principalSchema: "dbo",
                        principalTable: "Advertisements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AdvertisementViews_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
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
                name: "Customers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LocalGovernmentId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsSubscriptionActive = table.Column<bool>(type: "bit", nullable: false),
                    WaivedProductId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Customers_LocalGovernments_LocalGovernmentId",
                        column: x => x.LocalGovernmentId,
                        principalSchema: "dbo",
                        principalTable: "LocalGovernments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "dbo",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Customers_WaivedProducts_WaivedProductId",
                        column: x => x.WaivedProductId,
                        principalSchema: "dbo",
                        principalTable: "WaivedProducts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductCategoryMappings",
                schema: "dbo",
                columns: table => new
                {
                    CategoriesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductsId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategoryMappings", x => new { x.CategoriesId, x.ProductsId });
                    table.ForeignKey(
                        name: "FK_ProductCategoryMappings_ProductCategories_CategoriesId",
                        column: x => x.CategoriesId,
                        principalSchema: "dbo",
                        principalTable: "ProductCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategoryMappings_WaivedProducts_ProductsId",
                        column: x => x.ProductsId,
                        principalSchema: "dbo",
                        principalTable: "WaivedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LevyPayments",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChairmanId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MarketId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TraderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Period = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    TransactionReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HasIncentive = table.Column<bool>(type: "bit", nullable: false),
                    IncentiveAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GoodBoyId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CollectionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QRCodeScanned = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LevyPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LevyPayments_Chairmen_ChairmanId",
                        column: x => x.ChairmanId,
                        principalSchema: "dbo",
                        principalTable: "Chairmen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LevyPayments_GoodBoys_GoodBoyId",
                        column: x => x.GoodBoyId,
                        principalSchema: "dbo",
                        principalTable: "GoodBoys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LevyPayments_Markets_MarketId",
                        column: x => x.MarketId,
                        principalSchema: "dbo",
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LevyPayments_Traders_TraderId",
                        column: x => x.TraderId,
                        principalSchema: "dbo",
                        principalTable: "Traders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerFeedbacks",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerFeedbacks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerFeedbacks_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "dbo",
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerOrders",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VendorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CustomerId1 = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerOrders_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "dbo",
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerOrders_Customers_CustomerId1",
                        column: x => x.CustomerId1,
                        principalSchema: "dbo",
                        principalTable: "Customers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CustomerOrders_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "dbo",
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerOrderItems",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OrderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    WaivedProductId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerOrderItems_CustomerOrders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "dbo",
                        principalTable: "CustomerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerOrderItems_WaivedProducts_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "WaivedProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admins_UserId",
                schema: "dbo",
                table: "Admins",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementLanguages_AdvertisementId",
                schema: "dbo",
                table: "AdvertisementLanguages",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_AdminId",
                schema: "dbo",
                table: "Advertisements",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_VendorId",
                schema: "dbo",
                table: "Advertisements",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Advertisements_VendorId1",
                schema: "dbo",
                table: "Advertisements",
                column: "VendorId1");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementViews_AdvertisementId",
                schema: "dbo",
                table: "AdvertisementViews",
                column: "AdvertisementId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertisementViews_UserId",
                schema: "dbo",
                table: "AdvertisementViews",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertPayments_AdvertisementId",
                schema: "dbo",
                table: "AdvertPayments",
                column: "AdvertisementId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                schema: "dbo",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                schema: "dbo",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                schema: "dbo",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                schema: "dbo",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistCenterOfficers_ChairmanId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "ChairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistCenterOfficers_LocalGovernmentId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "LocalGovernmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistCenterOfficers_MarketId",
                schema: "dbo",
                table: "AssistCenterOfficers",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_AssistCenterOfficers_UserId",
                schema: "dbo",
                table: "AssistCenterOfficers",
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
                name: "IX_Caretakers_ChairmanId",
                schema: "dbo",
                table: "Caretakers",
                column: "ChairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_Caretakers_MarketId",
                schema: "dbo",
                table: "Caretakers",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Caretakers_UserId",
                schema: "dbo",
                table: "Caretakers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chairmen_LocalGovernmentId",
                schema: "dbo",
                table: "Chairmen",
                column: "LocalGovernmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Chairmen_MarketId",
                schema: "dbo",
                table: "Chairmen",
                column: "MarketId",
                unique: true,
                filter: "[MarketId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Chairmen_UserId",
                schema: "dbo",
                table: "Chairmen",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_CustomerId",
                schema: "dbo",
                table: "CustomerFeedbacks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerFeedbacks_VendorId",
                schema: "dbo",
                table: "CustomerFeedbacks",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderItems_OrderId",
                schema: "dbo",
                table: "CustomerOrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrderItems_ProductId",
                schema: "dbo",
                table: "CustomerOrderItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerId",
                schema: "dbo",
                table: "CustomerOrders",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_CustomerId1",
                schema: "dbo",
                table: "CustomerOrders",
                column: "CustomerId1",
                unique: true,
                filter: "[CustomerId1] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerOrders_VendorId",
                schema: "dbo",
                table: "CustomerOrders",
                column: "VendorId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_LocalGovernmentId",
                schema: "dbo",
                table: "Customers",
                column: "LocalGovernmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_UserId",
                schema: "dbo",
                table: "Customers",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_WaivedProductId",
                schema: "dbo",
                table: "Customers",
                column: "WaivedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodBoys_CaretakerId",
                schema: "dbo",
                table: "GoodBoys",
                column: "CaretakerId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodBoys_MarketId",
                schema: "dbo",
                table: "GoodBoys",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_GoodBoys_UserId",
                schema: "dbo",
                table: "GoodBoys",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LevyPayments_ChairmanId",
                schema: "dbo",
                table: "LevyPayments",
                column: "ChairmanId");

            migrationBuilder.CreateIndex(
                name: "IX_LevyPayments_GoodBoyId",
                schema: "dbo",
                table: "LevyPayments",
                column: "GoodBoyId");

            migrationBuilder.CreateIndex(
                name: "IX_LevyPayments_MarketId",
                schema: "dbo",
                table: "LevyPayments",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_LevyPayments_TraderId",
                schema: "dbo",
                table: "LevyPayments",
                column: "TraderId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalGovernments_Name",
                schema: "dbo",
                table: "LocalGovernments",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Markets_LocalGovernmentId",
                schema: "dbo",
                table: "Markets",
                column: "LocalGovernmentId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketSections_MarketId",
                schema: "dbo",
                table: "MarketSections",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategoryMappings_ProductsId",
                schema: "dbo",
                table: "ProductCategoryMappings",
                column: "ProductsId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_Name",
                schema: "dbo",
                table: "RolePermissions",
                columns: new[] { "RoleId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                schema: "dbo",
                table: "Roles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SGId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SGId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriberId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriberId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionActivatorId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriptionActivatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionPlanId",
                schema: "dbo",
                table: "Subscriptions",
                column: "SubscriptionPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_CaretakerId",
                schema: "dbo",
                table: "Traders",
                column: "CaretakerId");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_MarketId",
                schema: "dbo",
                table: "Traders",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_QRCode",
                schema: "dbo",
                table: "Traders",
                column: "QRCode",
                unique: true,
                filter: "[QRCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_SectionId",
                schema: "dbo",
                table: "Traders",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Traders_TIN",
                schema: "dbo",
                table: "Traders",
                column: "TIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Traders_UserId",
                schema: "dbo",
                table: "Traders",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                schema: "dbo",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LocalGovernmentId",
                schema: "dbo",
                table: "Users",
                column: "LocalGovernmentId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                schema: "dbo",
                table: "Users",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_LocalGovernmentId",
                schema: "dbo",
                table: "Vendors",
                column: "LocalGovernmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_UserId",
                schema: "dbo",
                table: "Vendors",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendors_VendorCode",
                schema: "dbo",
                table: "Vendors",
                column: "VendorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaivedProducts_ProductCode",
                schema: "dbo",
                table: "WaivedProducts",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WaivedProducts_VendorId",
                schema: "dbo",
                table: "WaivedProducts",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdvertisementLanguages",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AdvertisementViews",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AdvertPayments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AssistCenterOfficers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CustomerFeedbacks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CustomerOrderItems",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LevyPayments",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductCategoryMappings",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "RolePermissions",
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
                name: "Subscriptions",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Advertisements",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "CustomerOrders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "GoodBoys",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Traders",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductCategories",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Roles",
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
                name: "SubscriptionPlans",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Admins",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Customers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Caretakers",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "MarketSections",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "SowFoodCompanies",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "WaivedProducts",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Chairmen",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Vendors",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Markets",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "LocalGovernments",
                schema: "dbo");
        }
    }
}
