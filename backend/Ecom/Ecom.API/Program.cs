using System.Security.Claims;
using Ecom.API;
using Ecom.API.Authentication;
using Ecom.API.Middleware;
using Ecom.Application;
using Ecom.infrastructure;
using Ecom.infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var authority = builder.Configuration["Authentication:Authority"]?.TrimEnd('/');
if (string.IsNullOrWhiteSpace(authority))
{
    throw new InvalidOperationException(
        "Authentication:Authority is not configured. Set it in appsettings.Development.json " +
        "for local development, or provide it through configuration or the " +
        "Authentication__Authority environment variable in other environments.");
}

var audience = builder.Configuration["Authentication:Audience"];
if (string.IsNullOrWhiteSpace(audience))
{
    throw new InvalidOperationException(
        "Authentication:Audience is not configured. Set it in appsettings.Development.json " +
        "for local development, or provide it through configuration or the " +
        "Authentication__Audience environment variable in other environments.");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "Ecom.API.xml");
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{authority}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{authority}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    ["openid"] = "OpenID Connect"
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = audience;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    var realmAccess = context.Principal.FindFirst("realm_access")?.Value;
                    KeycloakRoleClaims.AddRealmRoles(identity, realmAccess);
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AuthPolicies.Admin, policy => policy.RequireRole(AuthPolicies.AdminRole));
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.InfrastructureConfiguration(builder.Configuration);
builder.Services.AddApplication();
var app = builder.Build();

// Run before the exception handler so error responses carry the headers too.
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    // Dev-only seeding: HasData would run on every environment, including
    // Production. DbInitializer keeps this to the local dev database.
    using var scope = app.Services.CreateScope();
    await DbInitializer.SeedAsync(scope.ServiceProvider.GetRequiredService<AppDbContext>());

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("ecom-frontend");
        options.OAuthUsePkce();
    });
}

app.UseStatusCodePagesWithReExecute("/errors/{0}");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

/// <summary>
/// The application entry point, exposed for integration testing.
/// </summary>
public partial class Program { }
