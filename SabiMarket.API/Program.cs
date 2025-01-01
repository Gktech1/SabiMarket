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
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            var app = builder.Build();

            //using (var scope = app.Services.CreateScope())
            //{
            //    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            //    await seeder.SeedAsync();
            //};
            //MONITORING 
            app.UseMiddleware<RequestTimeLoggingMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            //app.UseCustomErrorHandling();
            app.UseMiddleware<ExceptionMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}