using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Ecom.API.Helper;
using Ecom.infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ecom.IntegrationTests;

/// <summary>
/// Covers the API hardening behaviour: unified error shape, security headers,
/// CORS policy, and the exception handler wiring through the real host.
/// </summary>
public class ApiHardeningTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<Program> _factory;

    public ApiHardeningTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var schema = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        schema.Database.EnsureCreated();
    }

    private HttpClient CreateClient(string environment = "Testing")
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("RateLimiting:PermitLimit", "1000");
            builder.UseSetting("RateLimiting:WindowSeconds", "60");
            ConfigureSqlite(builder);
        });

        return factory.CreateClient();
    }

    private void ConfigureSqlite(IWebHostBuilder builder) =>
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });

    [Fact]
    public async Task SecurityHeaders_PresentOnSuccessfulResponse()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        AssertSecurityHeaders(response);
    }

    [Fact]
    public async Task SecurityHeaders_PresentOnErrorResponse()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/api/categories/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        AssertSecurityHeaders(response);
    }

    [Fact]
    public async Task UnhandledException_Development_IncludesExceptionDetails()
    {
        using var client = CreateClient("Development");

        var response = await client.PostAsJsonAsync(
            "/api/categories",
            new { Name = "", Description = "desc" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        AssertSecurityHeaders(response);

        var body = await response.Content.ReadFromJsonAsync<ResponseAPI>(JsonOptions());
        Assert.NotNull(body);
        Assert.Equal(StatusCodes.Status400BadRequest, body.StatusCode);
        Assert.Contains("Category name is required", body.Message);
        Assert.Contains("at ", body.Message);
    }

    [Fact]
    public async Task DomainValidationException_NonDevelopment_HidesExceptionDetails()
    {
        var client = CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/categories",
            new { Name = "", Description = "desc" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ResponseAPI>(JsonOptions());
        Assert.NotNull(body);
        Assert.Equal(StatusCodes.Status400BadRequest, body.StatusCode);
        Assert.Contains("Category name is required", body.Message);
        Assert.DoesNotContain("at ", body.Message);
    }

    [Fact]
    public async Task Cors_AllowsConfiguredOrigin()
    {
        var client = CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
        request.Headers.Add("Origin", "http://localhost:4200");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin").First());
    }

    [Fact]
    public async Task Cors_RejectsOtherOrigin()
    {
        var client = CreateClient();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/products");
        request.Headers.Add("Origin", "http://evil.example");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static void AssertSecurityHeaders(HttpResponseMessage response)
    {
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
    }

    private static JsonSerializerOptions JsonOptions() =>
        new() { PropertyNameCaseInsensitive = true };

    public void Dispose()
    {
        _connection.Dispose();
    }
}
