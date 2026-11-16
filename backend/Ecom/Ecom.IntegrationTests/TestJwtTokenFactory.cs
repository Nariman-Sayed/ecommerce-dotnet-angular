using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Ecom.IntegrationTests;

internal sealed class TestJwtTokenFactory
{
    public const string Audience = "ecom-api";
    public const string Issuer = "https://issuer.ecom.test";

    private readonly SymmetricSecurityKey _signingKey = new(RandomNumberGenerator.GetBytes(32));

    public SecurityKey SigningKey => _signingKey;

    public static SecurityKey CreateDifferentSigningKey() =>
        new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));

    public string CreateToken(
        string role,
        string audience = Audience,
        DateTime? expires = null,
        SecurityKey? signingKey = null)
    {
        var realmAccess = JsonSerializer.Serialize(new { roles = new[] { role } });
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
            new Claim("preferred_username", "test-user"),
            new Claim("realm_access", realmAccess, JsonClaimValueTypes.Json)
        };

        var credentials = new SigningCredentials(
            signingKey ?? _signingKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: audience,
            claims: claims,
            expires: expires ?? DateTime.UtcNow.AddMinutes(5),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
