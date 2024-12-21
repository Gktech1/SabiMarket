using SabiMarket.Domain.Exceptions;
using SabiMarket.Domain.Models;
using SabiMarket.Infrastructure.Utilities;
using System.Net.Mime;
using System.Text.Json;

namespace SabiMarket.API.Middlewares
{
    public class ErrorHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "An error occurred processing {Path}", context.Request.Path);
                await HandleExceptionAsync(context, exception);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, errorDetails) = GetErrorDetails(exception);

            context.Response.ContentType = MediaTypeNames.Application.Json;
            context.Response.StatusCode = statusCode;

            var response = ResponseFactory.Fail<object>(
                errorDetails.Message,
                _environment.IsDevelopment() ? errorDetails : null
            );

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(json);
        }

        private (int StatusCode, ErrorDetails ErrorDetails) GetErrorDetails(Exception exception)
        {
            return exception switch
            {
                ApiException apiException => (
                    apiException.StatusCode,
                    new ErrorDetails
                    {
                        Type = exception.GetType().Name,
                        Message = apiException.Message,
                        StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null,
                        Errors = (exception as ValidationException)?.Errors
                    }
                ),

                // Handle specific framework exceptions
                InvalidOperationException => (
                    StatusCodes.Status400BadRequest,
                    new ErrorDetails
                    {
                        Type = "InvalidOperation",
                        Message = "Invalid operation performed",
                        StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
                    }
                ),

                // Default case for unhandled exceptions
                _ => (
                    StatusCodes.Status500InternalServerError,
                    new ErrorDetails
                    {
                        Type = "UnhandledException",
                        Message = _environment.IsDevelopment()
                            ? exception.Message
                            : "An unexpected error occurred",
                        StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
                    }
                )
            };
        }
    }
}