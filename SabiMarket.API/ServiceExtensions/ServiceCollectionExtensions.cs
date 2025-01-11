using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.IRepositories;
using SabiMarket.Application.Validators;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Repositories;
using SabiMarket.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 10;
            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddRoleManager<RoleManager<ApplicationRole>>();

        // Register Services
        services.AddScoped<DatabaseSeeder>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IValidator<RegistrationRequestDto>, RegistrationRequestValidator>();
        services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();
        services.AddScoped<IRepositoryManager, RepositoryManager>();
        services.AddScoped<IServiceManager, ServiceManager>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();

        return services;
    }
}