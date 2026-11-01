using Ecom.API.Helper;
using Ecom.API.Options;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Ecom.API.Extensions;

/// <summary>
/// Registers the fixed-window rate limiter used by the API.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// The name of the rate limiting policy applied to controllers.
    /// </summary>
    public const string FixedWindowPolicy = "fixed-window";

    /// <summary>
    /// Adds the configured fixed-window rate limiter.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddApiRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .BindConfiguration(RateLimitingOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var settings = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(FixedWindowPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = settings.PermitLimit,
                        Window = TimeSpan.FromSeconds(settings.WindowSeconds),
                        QueueLimit = settings.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(
                    new ResponseAPI(
                        StatusCodes.Status429TooManyRequests,
                        "Too many requests. Please retry later."),
                    cancellationToken);
            };
        });

        return services;
    }
}
