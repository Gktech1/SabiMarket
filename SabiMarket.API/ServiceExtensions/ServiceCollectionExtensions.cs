using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.Validators;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Services;

namespace SabiMarket.API.ServiceExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static async Task<IServiceCollection> AddDatabaseSeederAsync(this IServiceCollection services)
        {
            using var scope = services.BuildServiceProvider().CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();

            return services;
        }

        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // ... other registrations

            // Add Identity with Roles
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
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
            .AddRoleManager<RoleManager<IdentityRole>>(); // Ex

            // Register Services
            services.AddScoped<DatabaseSeeder>();
            services.AddScoped<IAuthenticationService, AuthenticationService>();

            // Register other services
            services.AddScoped<IValidator<RegistrationRequestDto>, RegistrationRequestValidator>();
            services.AddScoped<IValidator<LoginRequestDto>, LoginRequestValidator>();

            return services;
        }
    }
}
