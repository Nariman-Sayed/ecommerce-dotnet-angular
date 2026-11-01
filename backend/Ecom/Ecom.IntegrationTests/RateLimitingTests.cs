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
/// Covers rate limiting through the real host.
/// </summary>
public class RateLimitingTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly WebApplicationFactory<Program> _factory;

    public RateLimitingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var schema = new AppDbContext(
            new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        schema.Database.EnsureCreated();
    }

    [Fact]
    public async Task RequestsOverLimit_Return429WithUnifiedShape()
    {
        var factory = _factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.UseSetting("RateLimiting:PermitLimit", "2");
            builder.UseSetting("RateLimiting:WindowSeconds", "60");
            ConfigureSqlite(builder);
        });

        using var client = factory.CreateClient();

        var first = await client.GetAsync("/api/categories");
        var second = await client.GetAsync("/api/categories");
        var third = await client.GetAsync("/api/categories");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);

        var body = await third.Content.ReadFromJsonAsync<ResponseAPI>(JsonOptions());
        Assert.NotNull(body);
        Assert.Equal(StatusCodes.Status429TooManyRequests, body.StatusCode);
        Assert.Equal("Too many requests. Please retry later.", body.Message);
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

    private static JsonSerializerOptions JsonOptions() =>
        new() { PropertyNameCaseInsensitive = true };

    public void Dispose()
    {
        _connection.Dispose();
    }
}
