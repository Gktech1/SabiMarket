
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SabiMarket.API.Extensions;
using SabiMarket.API.Middlewares;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.Interfaces;
using SabiMarket.Application.Validators;
using SabiMarket.Domain.Entities.UserManagement;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Services;

namespace SabiMarket.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.


            //Add service extension
            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

            // Add DbContext
            builder.Services.AddDatabaseContext(builder.Configuration);
            // Add Identity
            /*builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();*/


            // Configure Identity options (optional)
           /* builder.Services.Configure<IdentityOptions>(options =>
            {
                // Password settings
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings
                options.User.RequireUniqueEmail = true;
            });*/

            // Add custom error handling
            builder.Services.AddCustomErrorHandling(); // Add this BEFORE var app = builder.Build()


            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<RequestTimeLoggingMiddleware>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

        /*    // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }*/

            //MONITORING 
            app.UseMiddleware<RequestTimeLoggingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
           
            app.UseCustomErrorHandling();

            app.UseAuthorization();

            // Seed the database
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                    await seeder.SeedAsync();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }


            app.MapControllers();

            app.Run();
        }
    }
}
