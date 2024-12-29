using Microsoft.EntityFrameworkCore;
using SabiMarket.API.Extensions;
using SabiMarket.API.Middlewares;
using SabiMarket.API.ServiceExtensions;
using SabiMarket.Application.Interfaces;
using SabiMarket.Infrastructure.Data;
using SabiMarket.Infrastructure.Services;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddApplicationServices(builder.Configuration);
        builder.Services.AddControllers();
        builder.Services.AddDatabaseContext(builder.Configuration);
        builder.Services.AddCustomErrorHandling();
        builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
        builder.Services.AddScoped<RequestTimeLoggingMiddleware>();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Database Migration and Seeding
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync(); // Ensure database is created and migrated

                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
            }
        }

        app.UseMiddleware<RequestTimeLoggingMiddleware>();
        // Middleware configuration
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        app.UseHttpsRedirection();
        app.UseCustomErrorHandling();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        await app.RunAsync();
    }
}


















/*
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
            *//*builder.Services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();*//*


            // Add custom error handling
            builder.Services.AddCustomErrorHandling(); // Add this BEFORE var app = builder.Build()

            await builder.Services.AddDatabaseSeederAsync();

            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<RequestTimeLoggingMiddleware>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            var app = builder.Build();

        *//*    // Seed database
            using (var scope = app.Services.CreateScope())
            {
                var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
                await seeder.SeedAsync();
            }*//*

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
            app.UseAuthentication();
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
*/