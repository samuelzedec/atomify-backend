using System.Globalization;
using System.Threading.RateLimiting;
using Admin.Api.Constants;
using Admin.Api.Middlewares;
using BuildingBlocks.Application.Results;
using Identity.Infrastructure;
using Microsoft.Extensions.Caching.Hybrid;
using RedisRateLimiting.AspNetCore;
using StackExchange.Redis;

namespace Admin.Api.Setups;

internal static class BuilderSetup
{
    extension(WebApplicationBuilder builder)
    {
        public void ConfigureSetup()
        {
            var redisConnection = builder.ConfigureCacheConnection();
            builder.ConfigureCaching(redisConnection);
            builder.ConfigureRateLimiting(redisConnection);
            builder.ConfigureExceptionHandler();
            builder.ConfigureApiDocumentation();
            builder.ConfigureModules();
        }

        private ConnectionMultiplexer ConfigureCacheConnection()
        {
            var redisConnection = ConnectionMultiplexer.Connect(
                builder.Configuration.GetValue<string>("Redis:Configuration")!);

            builder.Services.AddSingleton<IConnectionMultiplexer>(redisConnection);

            return redisConnection;
        }

        private void ConfigureCaching(IConnectionMultiplexer redisConnection)
        {
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(redisConnection);
                options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName");
            });

            builder.Services.AddHybridCache(options => options.DefaultEntryOptions = new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10), LocalCacheExpiration = TimeSpan.FromMinutes(2)
            });
        }

        private void ConfigureRateLimiting(IConnectionMultiplexer redisConnection)
        {
            builder.Services.AddRateLimiter(options =>
            {
                var windowSize = TimeSpan.FromMinutes(1);

                options.AddRedisSlidingWindowLimiter(RateLimitingConstants.Default, redisOptions =>
                {
                    redisOptions.ConnectionMultiplexerFactory = () => redisConnection;
                    redisOptions.PermitLimit = 100;
                    redisOptions.Window = windowSize;
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                        ? retryAfterValue.TotalSeconds
                        : windowSize.TotalSeconds;

                    context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString(CultureInfo.InvariantCulture);
                    await context.HttpContext.Response.WriteAsJsonAsync(new Error(
                            type: ErrorType.TooManyRequests,
                            message: $"Limite de requisições excedido. Tente novamente em {retryAfter} segundos."),
                        cancellationToken);
                };
            });
        }

        private void ConfigureExceptionHandler()
        {
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        }

        private void ConfigureApiDocumentation()
            => builder.Services.AddOpenApi();

        private void ConfigureModules()
        {
            builder.ConfigureIdentityModule();
        }
    }
}