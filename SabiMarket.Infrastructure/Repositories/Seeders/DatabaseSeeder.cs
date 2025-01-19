﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SabiMarket.Domain.Entities.Administration;
using SabiMarket.Domain.Entities;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Data;

public static class UserRoles
{
    public const string Admin = "ADMIN";
    public const string Vendor = "VENDOR";
    public const string Customer = "CUSTOMER";
    public const string Advertiser = "ADVERTISER";
    public const string Goodboy = "GOODBOY";                
    public const string AssistOfficer = "ASSIST_OFFICER";   
    public const string Chairman = "CHAIRMAN";
    public const string Caretaker = "CARETAKER";
    public const string Trader = "TRADER";
}
public class DatabaseSeeder
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ApplicationDbContext _context;

    public DatabaseSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IConfiguration configuration,
        ILogger<DatabaseSeeder> logger,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _logger = logger;
        _context = context;
    }

    public async Task SeedAsync()
    {
        try
        {
            await SeedRolesAsync();
            await SeedAdminUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedRolesAsync()
    {
        _logger.LogInformation("Seeding roles...");
        string[] roles = {
        UserRoles.Admin,
        UserRoles.Vendor,
        UserRoles.Customer,
        UserRoles.Advertiser,
        UserRoles.AssistOfficer,
        UserRoles.Goodboy,
        UserRoles.Trader,
        UserRoles.Chairman
    };
        foreach (var roleName in roles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new ApplicationRole(roleName)  // Changed from IdentityRole
                {
                    CreatedAt = DateTime.UtcNow,
                    Description = $"This is {roleName}",
                    IsActive = true   
                });
                _logger.LogInformation("Created role: {RoleName}", roleName);
            }
        }
    }

    private async Task SeedAdminUserAsync()
    {
        _logger.LogInformation("Seeding admin user...");
        var adminEmail = "admin@yourapp.com";
        var adminPassword = "YourSecurePassword123!";
        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            throw new InvalidOperationException("Admin credentials are not configured properly.");
        }
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                NormalizedEmail = adminEmail.ToUpper(),
                NormalizedUserName = adminEmail.ToUpper(),
                Address = "Default Admin Address",
                ProfileImageUrl = "default-admin-avatar.png",
                SecurityStamp = Guid.NewGuid().ToString(),
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                PhoneNumber = "+1234567890",
                PhoneNumberConfirmed = true,
                TwoFactorEnabled = false,
                LockoutEnabled = true,
                AccessFailedCount = 0,
                LastLoginAt = null
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, UserRoles.Admin);

                // Create Admin entity with dashboard access and stats
                var admin = new Admin
                {
                    UserId = adminUser.Id,
                    Position = "System Administrator",
                    Department = "IT Administration",
                    AdminLevel = "Super Admin",
                    HasDashboardAccess = true,
                    HasRoleManagementAccess = true,
                    HasTeamManagementAccess = true,
                    HasAuditLogAccess = true,
                    RegisteredLGAs = 0,
                    ActiveChairmen = 0,
                    TotalRevenue = 0,
                    StatsLastUpdatedAt = DateTime.UtcNow
                };

                _context.Admins.Add(admin);
                await _context.SaveChangesAsync();

                // Add initial audit log
                var auditLog = new AuditLog
                {
                    UserId = adminUser.Id,
                    Activity = "Admin user created",
                    Module = "Authentication",
                    Details = "Initial admin user created during system setup"
                };
                auditLog.SetDateTime(DateTime.UtcNow);

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();
            }
            /*  var result = await _userManager.CreateAsync(adminUser, adminPassword);
              if (result.Succeeded)
              {
                  await _userManager.AddToRoleAsync(adminUser, UserRoles.Admin);
                  _logger.LogInformation("Admin user created successfully");
              }*/
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create admin user. Errors: {errors}");
            }
        }
    }
}