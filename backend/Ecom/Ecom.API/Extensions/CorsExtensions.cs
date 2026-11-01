namespace Ecom.API.Extensions;

/// <summary>
/// Registers the CORS policy used by the API.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// The name of the CORS policy applied to the pipeline.
    /// </summary>
    public const string PolicyName = "AngularDev";

    /// <summary>
    /// Adds a CORS policy whose allowed origins are read from configuration.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddApiCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var origins = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
