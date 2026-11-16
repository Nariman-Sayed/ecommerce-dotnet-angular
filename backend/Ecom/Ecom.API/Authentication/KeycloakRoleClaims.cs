using System.Security.Claims;
using System.Text.Json;

namespace Ecom.API.Authentication;

/// <summary>
/// Translates Keycloak realm roles into standard role claims.
/// </summary>
public static class KeycloakRoleClaims
{
    /// <summary>
    /// Adds every role from the <c>realm_access.roles</c> claim to the identity.
    /// </summary>
    /// <param name="identity">The identity that receives the role claims.</param>
    /// <param name="realmAccessJson">The raw value of the <c>realm_access</c> claim.</param>
    public static void AddRealmRoles(ClaimsIdentity identity, string? realmAccessJson)
    {
        ArgumentNullException.ThrowIfNull(identity);

        if (string.IsNullOrWhiteSpace(realmAccessJson))
            return;

        using var document = JsonDocument.Parse(realmAccessJson);
        if (!document.RootElement.TryGetProperty("roles", out var roles) ||
            roles.ValueKind != JsonValueKind.Array)
            return;

        foreach (var role in roles.EnumerateArray())
        {
            var value = role.GetString();
            if (!string.IsNullOrWhiteSpace(value))
                identity.AddClaim(new Claim(ClaimTypes.Role, value));
        }
    }
}
