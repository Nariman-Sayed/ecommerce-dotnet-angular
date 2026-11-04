namespace Ecom.API;

/// <summary>
/// Authorization policy and role names used by the API.
/// </summary>
public static class AuthPolicies
{
    /// <summary>
    /// Policy that requires the admin role.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Keycloak realm role that grants administrative access.
    /// </summary>
    public const string AdminRole = "admin";
}
