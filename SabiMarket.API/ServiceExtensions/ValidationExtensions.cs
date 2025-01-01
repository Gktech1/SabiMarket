using FluentValidation;
using SabiMarket.Application.DTOs.Requests;
using SabiMarket.Application.Validators;

namespace SabiMarket.API.ServiceExtensions
{
    public static class ValidationExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped<IValidator<RegistrationRequestDto>, RegistrationRequestValidator>();

            // Optional: Register individual validators if needed separately
           /* services.AddScoped<IValidator<VendorDetailsDto>, VendorDetailsValidator>();
            services.AddScoped<IValidator<CustomerDetailsDto>, CustomerDetailsValidator>();
            services.AddScoped<IValidator<AdvertiserDetailsDto>, AdvertiserDetailsValidator>();*/

            return services;
        }
    }
}
