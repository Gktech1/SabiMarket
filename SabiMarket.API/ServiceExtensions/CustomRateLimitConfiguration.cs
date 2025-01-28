using AspNetCoreRateLimit;

namespace SabiMarket.API.ServiceExtensions
{
    public class CustomRateLimitConfiguration
    {
        public static void ConfigureRateLimiting(IServiceCollection services, IConfiguration configuration)
        {
            // Rate limit rules
            var rateLimitRules = new List<RateLimitRule>
        {
            new RateLimitRule
            {
                Endpoint = "*:/api/*",
                Period = "1m",
                Limit = 100,
            },
            new RateLimitRule
            {
                Endpoint = "*:/api/trader/*",
                Period = "15m",
                Limit = 250,
            }
        };

            // IP rate limiting options
            services.Configure<IpRateLimitOptions>(opt =>
            {
                opt.GeneralRules = rateLimitRules;
                opt.EnableEndpointRateLimiting = true;
                opt.StackBlockedRequests = false;
                opt.HttpStatusCode = 429;
                opt.RealIpHeader = "X-Real-IP";
                opt.ClientIdHeader = "X-ClientId";
            });
        }
    }
}
