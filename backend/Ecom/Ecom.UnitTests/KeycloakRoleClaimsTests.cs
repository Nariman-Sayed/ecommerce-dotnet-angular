using System.Security.Claims;
using Ecom.API.Authentication;

namespace Ecom.UnitTests;

public class KeycloakRoleClaimsTests
{
    [Fact]
    public void AddRealmRoles_WithRoles_AddsOneClaimPerRole()
    {
        var identity = new ClaimsIdentity();
        const string realmAccess = "{\"roles\":[\"admin\",\"customer\"]}";

        KeycloakRoleClaims.AddRealmRoles(identity, realmAccess);

        var roles = identity.FindAll(ClaimTypes.Role).Select(claim => claim.Value);
        Assert.Equal(new[] { "admin", "customer" }, roles);
    }

    [Fact]
    public void AddRealmRoles_WithoutRealmAccess_AddsNoClaims()
    {
        var identity = new ClaimsIdentity();

        KeycloakRoleClaims.AddRealmRoles(identity, null);

        Assert.Empty(identity.FindAll(ClaimTypes.Role));
    }

    [Fact]
    public void AddRealmRoles_WithoutRolesArray_AddsNoClaims()
    {
        var identity = new ClaimsIdentity();
        const string realmAccess = "{\"other\":\"value\"}";

        KeycloakRoleClaims.AddRealmRoles(identity, realmAccess);

        Assert.Empty(identity.FindAll(ClaimTypes.Role));
    }
}
