using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Ecom.IntegrationTests;

/// <summary>
/// Registers the deterministic authentication scheme used by integration tests.
/// </summary>
public static class TestAuthenticationExtensions
{
    /// <summary>
    /// Replaces the API's default authentication scheme with the test scheme.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddTestAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(TestAuthHandler.SchemeName)
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { });

        return services;
    }
}
