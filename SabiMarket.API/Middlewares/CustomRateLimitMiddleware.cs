namespace SabiMarket.API.Middlewares
{
    namespace SabiMarket.API.ServiceExtensions
    {
        public class CustomRateLimitMiddleware
        {
            private readonly RequestDelegate _next;
            private readonly ILogger<CustomRateLimitMiddleware> _logger;

            public CustomRateLimitMiddleware(RequestDelegate next, ILogger<CustomRateLimitMiddleware> logger)
            {
                _next = next;
                _logger = logger;
            }

            public async Task InvokeAsync(HttpContext context)
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex) when (context.Response.StatusCode == 429)
                {
                    _logger.LogWarning($"Rate limit exceeded for IP: {context.Connection.RemoteIpAddress}");

                    context.Response.ContentType = "application/json";
                    var response = new
                    {
                        Status = "Error",
                        Message = "Too many requests. Please try again later.",
                        RetryAfter = context.Response.Headers["Retry-After"]
                    };

                    await context.Response.WriteAsJsonAsync(response);
                }
            }
        }
    }

}
